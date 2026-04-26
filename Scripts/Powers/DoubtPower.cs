using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TsukiyukiMiyako.Scripts;

public sealed class DoubtPower : CustomPowerModel
{
    // 模组固定图标路径
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 下回合开始触发（对标 ReassuringTouchPower）
    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == base.Owner.Side)
        {
            Flash();
            // 失去能量（对标 Void 官方方法）
            await PlayerCmd.LoseEnergy(2, base.Owner.Player!);
            // 触发后移除
            await PowerCmd.Remove(this);
        }
    }
}