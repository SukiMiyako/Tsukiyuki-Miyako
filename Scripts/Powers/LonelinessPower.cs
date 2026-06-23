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
/// Each stack applies -1 Strength and -1 Dexterity via internal Strength/Dexterity powers.
/// Automatically removed when SenseiPower goes above 0.
/// </summary>
public sealed class LonelinessPower : MiyakoModPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // Apply negative Strength equal to the full stack count
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, silent: true);
        // Apply negative Dexterity equal to the full stack count
        await PowerCmd.Apply<DexterityPower>(
            new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, silent: true);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power != this)
            return;

        // When loneliness changes, adjust the underlying Strength/Dexterity
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, base.Owner, -amount, applier, cardSource, silent: true);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext, base.Owner, -amount, applier, cardSource, silent: true);
    }

}
