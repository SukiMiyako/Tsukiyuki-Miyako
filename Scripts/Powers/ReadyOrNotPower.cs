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
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Powers;

public sealed class ReadyOrNotPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    // 修复：ICombatState 对齐新版签名
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner.Player)
        {
            return;
        }

        for (int i = 0; i < base.Amount; i++)
        {
            CardModel cardModel = CardFactory.GetDistinctForCombat(
                player,
                from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                where c.CanonicalKeywords.Contains(MyKeywords.Equipment)
                select c,
                1,
                player.RunState.Rng.CombatCardGeneration).FirstOrDefault()!;

            if (cardModel != null)
            {
                cardModel.SetToFreeThisTurn();
                // 修复：照搬灾祸官方重载
                await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, player);
            }
        }
    }
}