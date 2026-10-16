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
/// 「该反击了」的状态：举起盾、等对面打完，然后决定要不要还手。
///
/// 逻辑：
///   1. 卡牌打出时挂上本 power，并用 `SetDamage` 把 20 点灌进来；
///   2. 期间只要**失去过生命值**（`AfterCurrentHpChanged` 收到负 delta）就记一笔 —— 被破防了就不反击；
///   3. 到你的**下个回合开始**（`AfterPlayerTurnStart`，也就是敌方行动结束之后）结算：
///      没掉过血 → 对所有敌人造成这份伤害；掉过血 → 安静地消失。
///
/// 为什么用 `AfterPlayerTurnStart` 而不是"打出时立刻判定"：
///   立刻判定的话，敌人还没动手，条件几乎白给；挂到下回合开始才是"扛住了才还手"。
///
/// 注意：判定的是**生命值变化**，不是"受到伤害"——被格挡住的攻击不会触发记录（这正是这张牌想要的）。
/// </summary>
public sealed class FightBackPower : MiyakoModPower
{
    public override PowerType Type => PowerType.Buff;

    // 布尔状态：不显示数字
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>本人这一轮有没有掉过血。</summary>
    private bool _tookHpLoss;

    /// <summary>来源卡牌（结算时当 cardSource 用）。</summary>
    private CardModel? _source;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move)
    ];

    public void SetDamage(decimal damage)
    {
        AssertMutable();
        base.DynamicVars.Damage.BaseValue = damage;
    }

    public void SetSource(CardModel source)
    {
        AssertMutable();
        _source = source;
    }

    // 图标：按项目命名约定找 tsukiyukimiyako-fight_back_power.png，缺图回落通用图标
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

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // 只记"掉血"，回血/被格挡都不算
        if (creature == base.Owner && delta < 0m)
        {
            _tookHpLoss = true;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 只认挂在自己身上的这一位玩家
        if (player != base.Owner.Player)
        {
            return;
        }

        if (!_tookHpLoss && !base.Owner.IsDead)
        {
            Flash();
            await CreatureCmd.Damage(
                choiceContext,
                base.CombatState!.HittableEnemies,
                base.DynamicVars.Damage.BaseValue,
                ValueProp.Move,
                base.Owner,
                _source);
        }

        await PowerCmd.Remove(this);
    }
}
