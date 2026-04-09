using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace TsukiyukiMiyako.Scripts;


public sealed class EnergyGalPower : CustomPowerModel
{
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/{Id.Entry.ToLowerInvariant()}.png";
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player == base.Owner.Player)
        {
            Flash();
            await PlayerCmd.GainEnergy(base.Amount, base.Owner.Player);
        }
    }
}