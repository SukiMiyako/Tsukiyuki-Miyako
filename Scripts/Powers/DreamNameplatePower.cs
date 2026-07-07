using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
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
/// Hidden power applied by OrigDreamUpgraded.
/// Watches SenseiPower: when stacks cross 5, grants/removes 2 Strength.
/// </summary>
public sealed class DreamNameplatePower : MiyakoModPower
{
    private bool _strengthGranted;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power is not SenseiPower)
            return;

        int total = (int)power.Amount;
        int oldTotal = total - (int)amount;

        if (oldTotal < 5 && total >= 5 && !_strengthGranted)
        {
            _strengthGranted = true;
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, base.Owner, 2, base.Owner, null);
        }
        else if (oldTotal >= 5 && total < 5 && _strengthGranted)
        {
            _strengthGranted = false;
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, base.Owner, -2, base.Owner, null);
        }
    }
}