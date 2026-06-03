using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TsukiyukiMiyako.Scripts;

public sealed class AmmoSurplusPower : CustomPowerModel
{
    // Internal state tracking for stars-spent triggers (mirrors Orbit's pattern)
    private class Data
    {
        public int starsSpent;
        public int triggerCount;
    }

    // Threshold: trigger every N stars spent
    private const int _starsIncrement = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // Remaining stars needed for the next trigger
    public override int DisplayAmount => 5 - GetInternalData<Data>().starsSpent % 5;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new StarsVar(1) };

    protected override object InitInternalData()
    {
        return new Data();
    }

    // AfterStarsSpent is called every time the player spends stars
    // Accumulates spending and grants bonus stars when the threshold is reached
    public override async Task AfterStarsSpent(int amount, Player spender)
    {
        if (amount > 0 && spender == base.Owner.Player)
        {
            Data data = GetInternalData<Data>();
            data.starsSpent += amount;

            int triggers = data.starsSpent / _starsIncrement - data.triggerCount;

            if (triggers > 0)
            {
                Flash();
                await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue * triggers, base.Owner.Player);
                data.triggerCount += triggers;
            }
            InvokeDisplayAmountChanged();
        }
    }
}