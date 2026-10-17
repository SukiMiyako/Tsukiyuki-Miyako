using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// 「这份触摸，令人安心」的延迟格挡。
///
/// 数值存在 `Amount` 里（不是 BlockVar）：
/// 能力提示框只会自动注入 `{Amount}` 这类"笨变量"；
/// 自定义动态变量必须同时有 .smartDescription + 战斗中实例才注入，
/// 而卡牌上的悬浮提示走的是 GetDumbHoverTip()，永远解析不了自定义变量
/// （见 PowerModel.cs:350-398，以及 HoverTipFactory.FromPower → GetDumbHoverTip）。
///
/// 所以：Amount = 本牌实际获得的格挡（含羁绊加成后的值），
/// 下回合开始时照这个数再给一次，然后自移除（不再用 Decrement 记回合数）。
/// </summary>
public sealed class ReassuringTouchPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    // 层数 = 下回合要给的格挡
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";

    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>() { HoverTipFactory.Static(StaticHoverTip.Block) };

    // （对齐）
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            Flash();
            await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
            await PowerCmd.Remove(this);
        }
    }
}
