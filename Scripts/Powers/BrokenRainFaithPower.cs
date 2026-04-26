using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

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

    // ===================== 免费卡牌逻辑（原版虚空形态写法） =====================
    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (ShouldSkip(card)) return false;
        modifiedCost = 0m;
        return true;
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (ShouldSkip(card)) return false;
        modifiedCost = 0m;
        return true;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && !cardPlay.IsAutoPlay && cardPlay.IsLastInSeries)
        {
            GetInternalData<Data>().CardsPlayedThisTurn++;
        }
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
    {
        if (side == base.Owner.Side)
            GetInternalData<Data>().CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    private bool ShouldSkip(CardModel card)
    {
        if (card.Owner.Creature != base.Owner) return true;
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play)) return true;
        return GetInternalData<Data>().CardsPlayedThisTurn >= base.Amount;
    }

    // ===================== 1:1 抄灾祸的卡牌生成写法（完美适配） =====================
    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != base.Owner.Side) return;

        var data = GetInternalData<Data>();
        data.TurnsCounter++;

        if (data.TurnsCounter >= 1)
        {
            data.TurnsCounter = 0;
            Flash();

            // 完全照搬灾祸的 GetForCombat + AddGeneratedCardToCombat 写法
            List<CardModel> doubtCards = CardFactory.GetForCombat(
                base.Owner.Player!,
                from c in base.Owner.Player!.Character.CardPool.GetUnlockedCards(
                    base.Owner.Player.UnlockState,
                    base.Owner.Player.RunState.CardMultiplayerConstraint)
                where c is Doubt
                select c,
                1,
                base.Owner.Player.RunState.Rng.CombatCardGeneration
            ).ToList();

            foreach (CardModel item in doubtCards)
            {
                // 灾祸原版写法：参数是 卡牌, 牌堆, 玩家
                await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Draw, base.Owner.Player);
            }
        }
    }
}