using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// Temporary strength buff applied by Rest card.
/// Grants Strength while active; removes it when the power expires at end of turn.
/// Mirrors the game's TemporaryStrengthPower pattern but extends MiyakoModPower
/// for custom icon support and proper mod localization.
/// </summary>
public sealed class TemporaryStrengthUpPower : MiyakoModPower
{
    private bool _shouldIgnoreNextInstance;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Strength", 2m)
    };

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
        }
        else
        {
            await PowerCmd.Apply<StrengthPower>(
                new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);
        }
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (amount != (decimal)base.Amount && power == this)
        {
            if (_shouldIgnoreNextInstance)
            {
                _shouldIgnoreNextInstance = false;
            }
            else
            {
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext, base.Owner, amount, applier, cardSource, silent: true);
            }
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            Flash();
            await PowerCmd.Remove(this);
            // Reverse: remove the granted strength
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -base.Amount, base.Owner, null);
        }
    }

    public void IgnoreNextInstance()
    {
        _shouldIgnoreNextInstance = true;
    }
}
