using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts;

public sealed class RestPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    // NightmarePower 核心：下回合触发（下回合逻辑）
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        // 仅自己的回合触发
        if (player == base.Owner.Player)
        {
            Flash();
            // 施加临时力量
            await PowerCmd.Apply<TemporaryStrengthUpPower>(choiceContext, base.Owner, base.Amount, base.Owner, null);
            // 获得1点能量
            await PlayerCmd.GainEnergy(1, base.Owner.Player!);
            // 触发后移除自身（和Nightmare一致）
            await PowerCmd.Remove(this);
        }
    }
}