using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TsukiyukiMiyako.Scripts.Cards;

namespace TsukiyukiMiyako.Scripts;

public sealed class BrokenRainFaithPower : CustomPowerModel
{
    private class Data
    {
        public int CardsPlayedThisTurn;
        public int TurnsCounter;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override object InitInternalData() => new Data();

    // ✅ 唯一费用修改：仅前N张牌 耗能=0（无叠加、无无限免费）
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        // 严格遵循限制：超出数量不免费
        if (ShouldSkip(card))
            return false;

        modifiedCost = 0m;
        return true;
    }

    // ✅ 删除所有冲突的费用修改（杜绝叠加免费）
    // 移除：TryModifyEnergyCostInCombatLate / TryModifyStarCost

    // ✅ 原版计数逻辑（完整保留，控制前N张免费）
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && !cardPlay.IsAutoPlay && cardPlay.IsLastInSeries)
        {
            GetInternalData<Data>().CardsPlayedThisTurn++;
        }
        return Task.CompletedTask;
    }

    // ✅ 原版回合重置（完整保留）
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Side)
            GetInternalData<Data>().CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    // ✅ 原版限制逻辑（完整保留，核心！）
    private bool ShouldSkip(CardModel card)
    {
        if (card.Owner.Creature != base.Owner) return true;
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play)) return true;
        return GetInternalData<Data>().CardsPlayedThisTurn >= base.Amount;
    }

    // ✅ 原版抽动摇逻辑（完整保留）
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Side || base.Owner.Player == null || combatState == null)
            return;

        var data = GetInternalData<Data>();
        data.TurnsCounter++;

        if (data.TurnsCounter >= 1)
        {
            data.TurnsCounter = 0;
            Flash();
            CardModel doubtCard = combatState.CreateCard<Doubt>(base.Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(doubtCard, PileType.Draw, base.Owner.Player, CardPilePosition.Random);
        }
    }
}