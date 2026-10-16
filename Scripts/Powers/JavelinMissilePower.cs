using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// 「标枪飞弹」的锁定标记 —— 挂在**被锁定的敌人**身上（Owner = 敌人，不是施法者）。
///
/// 数据流（照原版 TheBombPower 的写法）：
///   卡牌 OnPlay → PowerCmd.Apply&lt;本 power&gt;(目标敌人) → SetDamage(卡面数值) + SetSource(卡牌)
///   → 下个玩家回合开始时：播发射音 → 等 0.75s（飞行时间）→ 单段结算 → 自移除
///
/// 几点说明：
/// - `InstanceType = Instanced`：每打出一张标枪就是一个独立实例，互不覆盖（否则两张牌会互相顶掉）。
/// - `StackType = Single`：锁定是"有/没有"的状态，图标上不该出现数字。
/// - 用 `AfterPlayerTurnStart` 而不是回合结束：卡牌是在玩家回合打出的，
///   所以下一个 `AfterPlayerTurnStart` 就是"下回合开始"，正好是一次引导。
/// - 结算时把卡牌当 cardSource 传进去，且 `CreatureCmd.Damage(…, DamageVar, CardModel)` 这个重载
///   会把 dealer 自动取成 `cardSource.Owner.Creature` —— 力量等加成口径和卡牌面板一致。
/// - 0.75s 的等待放在**伤害之前、Remove 之前**：`PowerCmd.Remove` 内部自带 0.2~0.4s 等待，
///   若先 Remove 再等，音效到出伤就会变成 1.0~1.15s，对不上。
/// - 目标死亡/离场时 power 会被引擎随主人清理（默认行为），不需要额外收拾。
/// </summary>
public sealed class JavelinMissilePower : MiyakoModPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(17m, ValueProp.Move)
    ];

    /// <summary>打出这张牌的卡（结算时当 cardSource 用）。</summary>
    private CardModel? _source;

    /// <summary>
    /// 把卡面伤害写进本实例。必须走 setter —— Amount 没被占用，但照 TheBombPower 的约定，
    /// 数值放在 DynamicVars 里、由卡牌在 Apply 之后灌进来。
    /// </summary>
    public void SetDamage(decimal damage)
    {
        AssertMutable();
        base.DynamicVars.Damage.BaseValue = damage;
    }

    /// <summary>记住来源卡牌：结算时当 cardSource，保证吃到的加成和卡牌面板一致。</summary>
    public void SetSource(CardModel source)
    {
        AssertMutable();
        _source = source;
    }

    // 图标：按项目命名约定找 tsukiyukimiyako-javelin_missile_power.png，缺图回落到通用图标
    public override string CustomPackedIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/powers/power.png";
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/powers/big/power.png";
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 只认打出这张牌的那位玩家（多人局里别人的回合开始不该替我发射）
        if (_source == null || player != _source.Owner)
        {
            return;
        }

        // 目标已经死了：引擎会随主人死亡清理掉这个 power，这里直接收工
        if (base.Owner.IsDead)
        {
            return;
        }

        // 发射
        SfxCmd.Play("res://Audio/javalin fire.mp3");

        // 飞行时间
        await Cmd.Wait(0.75f);

        // 飞行途中目标死了：别再补刀（power 此时已被引擎清理）
        if (base.Owner.IsDead)
        {
            return;
        }

        // 命中：单段，dealer 由 cardSource 推导
        await CreatureCmd.Damage(choiceContext, base.Owner, base.DynamicVars.Damage, _source);

        await PowerCmd.Remove(this);
    }
}
