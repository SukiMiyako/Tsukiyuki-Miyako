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

    // ===================== 虚空形态·免费卡牌逻辑（完全不变） =====================
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
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

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && !cardPlay.IsAutoPlay && cardPlay.IsLastInSeries)
        {
            GetInternalData<Data>().CardsPlayedThisTurn++;
        }
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side == base.Owner.Side) GetInternalData<Data>().CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    private bool ShouldSkip(CardModel card)
    {
        if (card.Owner.Creature != base.Owner) return true;
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play)) return true;
        return GetInternalData<Data>().CardsPlayedThisTurn >= base.Amount;
    }

    // ===================== 开心小花·三回合计数 =====================
    // ===================== 【仅这里：1:1抄灾祸的卡牌生成方式】 =====================
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != base.Owner.Side) return;

        var data = GetInternalData<Data>();
        data.TurnsCounter++;

        if (data.TurnsCounter >= 3)
        {
            data.TurnsCounter = 0;
            Flash();

            // ↓↓↓ 纯抄灾祸 CalamityPower 生成卡牌代码，仅修改为生成【动摇】↓↓↓
            List<CardModel> doubtCards = CardFactory.GetForCombat(base.Owner.Player!,
                from c in base.Owner.Player!.Character.CardPool.GetUnlockedCards(base.Owner.Player.UnlockState, base.Owner.Player.RunState.CardMultiplayerConstraint)
                where c is Doubt // 筛选指定卡牌：动摇
                select c,
                1, // 生成1张
                base.Owner.Player.RunState.Rng.CombatCardGeneration).ToList();

            foreach (CardModel item in doubtCards)
            {
                await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Draw, addedByPlayer: true);
            }
        }
    }
}