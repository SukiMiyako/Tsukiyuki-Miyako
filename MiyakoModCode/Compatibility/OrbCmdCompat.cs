using System;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Tsukiyuki_Miyako.MiyakoModCode.Compatibility;

/// <summary>
/// Compatibility layer for <see cref="OrbCmd"/> methods whose signatures differ between
/// the public main branch and the public-beta branch.
/// </summary>
public static class OrbCmdCompat
{
    private static readonly MethodInfo? PassiveWithoutHookFlag = typeof(OrbCmd).GetMethod(
        nameof(OrbCmd.Passive),
        BindingFlags.Public | BindingFlags.Static,
        binder: null,
        types: [typeof(PlayerChoiceContext), typeof(OrbModel), typeof(Creature)],
        modifiers: null);

    private static readonly MethodInfo? PassiveWithHookFlag = typeof(OrbCmd).GetMethod(
        nameof(OrbCmd.Passive),
        BindingFlags.Public | BindingFlags.Static,
        binder: null,
        types: [typeof(PlayerChoiceContext), typeof(OrbModel), typeof(Creature), typeof(bool)],
        modifiers: null);

    /// <summary>
    /// Triggers an orb passive using the overload available on the running game branch.
    /// </summary>
    public static Task Passive(PlayerChoiceContext context, OrbModel orb, Creature? target)
    {
        if (PassiveWithHookFlag != null)
        {
            return (Task)PassiveWithHookFlag.Invoke(null, [context, orb, target, false])!;
        }

        if (PassiveWithoutHookFlag != null)
        {
            return (Task)PassiveWithoutHookFlag.Invoke(null, [context, orb, target])!;
        }

        throw new MissingMethodException("Unable to find a compatible OrbCmd.Passive method.");
    }
}
