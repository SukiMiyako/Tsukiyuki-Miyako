using System.Collections.Generic;
using Godot;
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

/// <summary>
/// Preloads Architect visual assets before the event room initializes.
/// Without this, the Architect creature has no visual and attack VFX cannot render.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "SetInitialEventState")]
public static class MiyakoArchitectPreloadPatch
{
    private static readonly string[] _preloadAssets =
    {
        "res://scenes/creature_visuals/architect.tscn",
        "res://scenes/backgrounds/the_architect_event_encounter/the_architect_event_encounter_background.tscn",
        "res://scenes/backgrounds/the_architect_event_encounter/layers/the_architect_event_encounter_bg_00_a.tscn",
    };

    public static void Prefix()
    {
        foreach (string path in _preloadAssets)
        {
            if (ResourceLoader.Exists(path))
            {
                ResourceLoader.Load(path);
            }
        }
    }
}
