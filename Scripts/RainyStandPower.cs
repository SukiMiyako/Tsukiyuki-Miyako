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

public sealed class RainyStandPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 核心：抽牌前触发，获得随机能力牌（完全复刻CreativeAi）
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (player != base.Owner.Player)
        {
            return;
        }

        // 每层获得1张能力牌
        for (int i = 0; i < base.Amount; i++)
        {
            CardModel cardModel = CardFactory.GetDistinctForCombat(player, from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                                                                           where c.Type == CardType.Power
                                                                           select c, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault()!;
            if (cardModel != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
            }
        }
    }
}