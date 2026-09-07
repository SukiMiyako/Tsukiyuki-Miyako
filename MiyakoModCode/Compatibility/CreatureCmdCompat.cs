using System;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Tsukiyuki_Miyako.MiyakoModCode.Compatibility;

/// <summary>
/// Small compatibility layer for APIs whose signatures differ between the public main branch
/// and the public-beta branch.
/// </summary>
public static class CreatureCmdCompat
{
    private static readonly MethodInfo? LoseBlockWithContext = typeof(CreatureCmd).GetMethod(
        nameof(CreatureCmd.LoseBlock),
        BindingFlags.Public | BindingFlags.Static,
        binder: null,
        types: [typeof(PlayerChoiceContext), typeof(Creature), typeof(decimal), typeof(Creature)],
        modifiers: null);

    private static readonly MethodInfo? LoseBlockWithoutContext = typeof(CreatureCmd).GetMethod(
        nameof(CreatureCmd.LoseBlock),
        BindingFlags.Public | BindingFlags.Static,
        binder: null,
        types: [typeof(Creature), typeof(decimal)],
        modifiers: null);

    /// <summary>
    /// Removes block using the CreatureCmd.LoseBlock overload available on the running game branch.
    /// </summary>
    public static Task LoseBlock(PlayerChoiceContext context, Creature target, decimal amount, Creature? remover)
    {
        if (LoseBlockWithContext != null)
        {
            return (Task)LoseBlockWithContext.Invoke(null, [context, target, amount, remover])!;
        }

        if (LoseBlockWithoutContext != null)
        {
            return (Task)LoseBlockWithoutContext.Invoke(null, [target, amount])!;
        }

        throw new MissingMethodException("Unable to find a compatible CreatureCmd.LoseBlock method.");
    }
}
