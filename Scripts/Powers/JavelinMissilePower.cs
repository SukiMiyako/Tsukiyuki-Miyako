using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// 「标枪飞弹」的锁定标记 —— 挂在**被锁定的敌人**身上（Owner = 敌人，不是施法者）。
///
/// 数据流：卡牌 OnPlay → Apply&lt;本 power&gt;(目标敌人, amount = 卡面伤害)
///        → 下个玩家回合开始：播发射音 → 等 0.75s（飞行时间）→ 单段结算 → 自移除
///
/// **伤害存在 `Amount` 里**（不是自定义 DamageVar）：
/// 能力提示框只会自动注入 `{Amount}` 这类"笨变量"；
/// 自定义动态变量必须同时满足"有 .smartDescription key" + "战斗中实例"才会注入
/// （见 PowerModel.cs:350-398 的 HoverTips，else 分支只调 AddDumbVariablesToDescription）。
/// 用 Amount 的话：图标层数会直接显示"17"（看得见这一发多重），描述里写 `{Amount}` 两条分支都能解析。
///
/// `InstanceType = Instanced`：每张标枪一个独立实例，互不覆盖，各带各的 Amount。
/// </summary>
public sealed class JavelinMissilePower : MiyakoModPower
{
    public override PowerType Type => PowerType.Debuff;

    // 层数 = 这一发标的伤害，直接显示在敌人头上
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>打出这张牌的卡（结算时当 cardSource 用）。</summary>
    private CardModel? _source;

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

        // 命中：单段，dealer 取卡主，cardSource 取原卡（加成口径与卡面一致）
        await CreatureCmd.Damage(
            choiceContext,
            base.Owner,
            base.Amount,
            ValueProp.Move,
            _source.Owner.Creature,
            _source);

        await PowerCmd.Remove(this);
    }
}
