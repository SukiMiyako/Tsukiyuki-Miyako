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

    // Makes the first N cards played each turn cost 0 energy
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (ShouldSkip(card))
            return false;

        modifiedCost = 0m;
        return true;
    }

    // Track cards played this turn to enforce the per-turn free-card limit
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && !cardPlay.IsAutoPlay && cardPlay.IsLastInSeries)
        {
            GetInternalData<Data>().CardsPlayedThisTurn++;
        }
        return Task.CompletedTask;
    }

    // Reset the per-turn counter at the start of the player's turn
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Side)
            GetInternalData<Data>().CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    // Only the owner's cards in hand/play pile are eligible; stops at the per-turn limit
    private bool ShouldSkip(CardModel card)
    {
        if (card.Owner.Creature != base.Owner) return true;
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play)) return true;
        return GetInternalData<Data>().CardsPlayedThisTurn >= base.Amount;
    }

    // Shuffle a Doubt card into the draw pile at the start of each player turn
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