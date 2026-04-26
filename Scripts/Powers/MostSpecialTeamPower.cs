using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TsukiyukiMiyako.Scripts;

public sealed class MostSpecialTeamPower : CustomPowerModel
{
    private class Data
    {
        public List<CardModel> GeneratedCards { get; set; } = new List<CardModel>();
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override object InitInternalData() => new Data();

    // 修复：CombatState → ICombatState（官方更新强制要求）
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner.Player)
            return;

        var data = GetInternalData<Data>();
        data.GeneratedCards.Clear();

        CardModel card = CardFactory.GetDistinctForCombat(player,
            from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            where c.CanonicalKeywords.Contains(MyKeywords.Support)
            select c, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault()!;

        if (card != null)
        {
            card.SetToFreeThisTurn();
            // 修复：照搬灾祸官方写法，无报错参数
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner.Player);
            data.GeneratedCards.Add(card);
        }
    }

    // ✅【纯官方原版】直接用你给的 OneTwoPunch 签名！一字不差！
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == base.Owner.Side)
        {
            var data = GetInternalData<Data>();
            foreach (var card in data.GeneratedCards)
            {
                if (card != null)
                    await CardPileCmd.RemoveFromCombat(card);
            }
            data.GeneratedCards.Clear();
        }
    }
}