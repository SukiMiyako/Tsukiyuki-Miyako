using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// Dual-purpose buff: each stack adds flat Block to card/monster-move block actions
/// and acts as Focus for powered orbs. Based on UnmovablePower.
/// </summary>
public sealed class SenseiPower : CustomPowerModel
{
    // 基础属性（标准配置）
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 允许为负：羁绊到 0 时**不再被移除**，负数可以长期存在，形成"负债羁绊"状态。
    /// 原因：`PowerModel.ApplyInternal` 对负数额度不调用 `ShouldRemoveDueToAmount()`，
    /// 而 `ModifyAmount` 会用"先删后建"的方式让负数实例残留 —— 与其依赖那条不可预期的路径，
    /// 不如显式允许负值，让寂寞的 `3 + |羁绊|` 有确定意义（越欠越痛）。
    /// </summary>
    public override bool AllowNegative => true;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";
    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null)
        {
            if (cardSource.Owner.Creature != base.Owner)
            {
                return 0m;
            }
        }
        else if (base.Owner != target)
        {
            return 0m;
        }
        if (!props.IsPoweredCardOrMonsterMoveBlock())
        {
            return 0m;
        }
        // 允许为负之后，负数羁绊会削减格挡 —— 用 0 兜底，避免把格挡加成吃成负收益
        return Math.Max(base.Amount, 0m);
    }

    public override decimal ModifyOrbValue(OrbModel orb, decimal value)
    {
        if (base.Owner.Player != orb.Owner)
            return value;

        return Math.Max(value + (decimal)base.Amount, 0m);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power != this)
            return;

        // Only apply loneliness to the owner of this SenseiPower
        Creature owner = base.Owner;
        int totalSensei = (int)base.Amount;

        if (totalSensei <= 0)
        {
            // Loneliness stacks = 3 + abs(SenseiPower)
            int lonelinessStacks = 3 + Math.Abs(totalSensei);
            int currentLoneliness = owner.GetPowerAmount<LonelinessPower>();
            int delta = lonelinessStacks - currentLoneliness;

            if (delta != 0)
            {
                // Apply delta — LonelinessPower.AfterPowerAmountChanged handles str/dex adjustment
                await PowerCmd.Apply<LonelinessPower>(
                    choiceContext, owner, delta, owner, null, silent: true);
            }
        }
        else
        {
            // SenseiPower > 0 → bring LonelinessPower to 0 (auto-cleanup via AfterPowerAmountChanged)
            int currentLoneliness = owner.GetPowerAmount<LonelinessPower>();
            if (currentLoneliness > 0)
            {
                await PowerCmd.Apply<LonelinessPower>(
                    choiceContext, owner, -currentLoneliness, owner, null, silent: true);
            }
        }
    }
}