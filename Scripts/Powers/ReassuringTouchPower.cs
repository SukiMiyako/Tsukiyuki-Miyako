using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace TsukiyukiMiyako.Scripts;

public sealed class ReassuringTouchPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>() { HoverTipFactory.Static(StaticHoverTip.Block) };
    public override bool IsInstanced => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>() { new BlockVar(0m, ValueProp.Unpowered) };

    public void SetBlock(decimal block)
    {
        AssertMutable();
        base.DynamicVars.Block.BaseValue = block;
    }

    // 核心修正：下回合开始触发（你要的效果）
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == base.Owner.Side)
        {
            Flash();
            await CreatureCmd.GainBlock(base.Owner, base.DynamicVars.Block, null);
            await PowerCmd.Decrement(this);
        }
    }
}