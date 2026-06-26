using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// Debuff applied when SenseiPower drops to 0 or below.
/// Each stack applies -1 Strength and -1 Dexterity.
/// Automatically removed when SenseiPower goes above 0.
///
/// Uses AfterPowerAmountChanged (not BeforeApplied) to manage underlying
/// Strength/Dexterity, because BeforeApplied fires for the full amount and
/// AfterPowerAmountChanged fires for the delta — using both would double-apply.
/// </summary>
public sealed class LonelinessPower : MiyakoModPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power != this)
            return;

        // `amount` is the CHANGE, not the total. Apply the delta to Strength/Dexterity.
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, base.Owner, -amount, applier, cardSource, silent: true);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext, base.Owner, -amount, applier, cardSource, silent: true);
    }
}
