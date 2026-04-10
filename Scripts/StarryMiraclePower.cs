using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts;

public sealed class StarryMiraclePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return Task.CompletedTask;

        return Task.CompletedTask;
    }

    // 打出能力牌 → 获得1层师生羁绊
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Type == CardType.Power)
        {
            Flash();
            await PowerCmd.Apply<SenseiPower>(base.Owner, 1m, base.Owner, null);
        }
    }
}