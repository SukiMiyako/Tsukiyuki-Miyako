using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Patches;

/// <summary>
/// Injects Miyako's character key into the Architect's dialogue set so that
/// the attack VFX and dialogue triggers work during the Architect ancient event.
///
/// Without this, GetValidDialogues() returns empty for Miyako because
/// DefineDialogues() only registers dialogues for built-in characters.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
public static class MiyakoArchitectPatch
{
    public static void Postfix(ref AncientDialogueSet __result)
    {
        string miyakoKey = ModelDb.Character<MiyakoMod>().Id.Entry;

        // Don't overwrite if Miyako already has an entry
        if (__result.CharacterDialogues.ContainsKey(miyakoKey))
            return;

        // Copy any existing character's dialogues as the template for Miyako.
        // The Architect's CharacterDialogues all have attack triggers (EndAttackers/StartAttackers),
        // so copying any of them gives Miyako the full attack experience.
        var template = __result.CharacterDialogues.FirstOrDefault().Value;
        if (template != null)
        {
            __result.CharacterDialogues[miyakoKey] = template;
        }
    }
}
