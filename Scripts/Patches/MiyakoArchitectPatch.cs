using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Patches;

/// <summary>
/// Injects Miyako's character key into the Architect's CharacterDialogues dictionary
/// and ensures Architect visual assets are preloaded.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
public static class MiyakoArchitectPatch
{
    public static void Postfix(ref AncientDialogueSet __result)
    {
        string miyakoKey = ModelDb.Character<MiyakoMod>().Id.Entry;

        if (__result.CharacterDialogues.ContainsKey(miyakoKey))
            return;

        // 3 repeating dialogues, 3 lines each (as defined in ancients.json).
        var miyakoDialogues = new AncientDialogue[]
        {
            new AncientDialogue("", "", "")
            {
                VisitIndex = null,
                StartAttackers = ArchitectAttackers.Player,
                EndAttackers = ArchitectAttackers.Both,
            },
            new AncientDialogue("", "", "")
            {
                VisitIndex = null,
                StartAttackers = ArchitectAttackers.Player,
                EndAttackers = ArchitectAttackers.Architect,
            },
            new AncientDialogue("", "", "")
            {
                VisitIndex = null,
                StartAttackers = ArchitectAttackers.Player,
                EndAttackers = ArchitectAttackers.Architect,
            },
        };

        __result.CharacterDialogues[miyakoKey] = miyakoDialogues;
    }
}

