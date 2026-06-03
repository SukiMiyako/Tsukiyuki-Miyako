using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TsukiyukiMiyako.Scripts;

public sealed class DroneSyncPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 只触发自己的攻击牌
        if (cardPlay.Card.Owner.Creature != Owner)
            return;

        if (cardPlay.Card.Type != CardType.Attack)
            return;

        // 你给的 LoopPower 写法
        if (Owner.Player!.PlayerCombatState!.OrbQueue.Orbs.Count != 0)
        {
            await OrbCmd.Passive(context, Owner.Player.PlayerCombatState.OrbQueue.Orbs[0], null);
        }
    }
}