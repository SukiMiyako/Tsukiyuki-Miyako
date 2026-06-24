using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Patches;

/// <summary>
/// Injects Miyako's character key into the Architect's CharacterDialogues dictionary.
/// The dialogue text is already defined in ancients.json under TSUKIYUKI_MIYAKO-MIYAKO_MOD keys.
/// This patch creates the AncientDialogue entries with proper line counts and attack triggers.
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
        // Each "" in the constructor represents one dialogue line (SFX path = no SFX).
        // Attackers: Player attacks at line transition, Architect attacks at dialogue end.
        var miyakoDialogues = new AncientDialogue[]
        {
            new AncientDialogue("", "", "")
            {
                VisitIndex = null, // repeating pool
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
