using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;

namespace TsukiyukiMiyako.Scripts.Patches;

/// <summary>
/// Harmony patches that intercept the game's Spine2D animation triggers and
/// redirect them to MiyakoAnimationDriver for AnimatedSprite2D-driven characters.
/// These are completely passive — they immediately return true for non-Miyako creatures.
/// </summary>

// Patch 1: Intercept SetAnimationTrigger → drive AnimatedSprite2D
[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
public static class MiyakoSetAnimationTriggerPatch
{
    public static void Postfix(NCreature __instance, string trigger)
    {
        if (MiyakoAnimationDriver.IsSpriteDriven(__instance))
        {
            MiyakoAnimationDriver.Trigger(__instance, trigger);
        }
    }
}

// Patch 2: Intercept ImmediatelySetIdle → play idle on AnimatedSprite2D
[HarmonyPatch(typeof(NCreature), "ImmediatelySetIdle")]
public static class MiyakoImmediatelySetIdlePatch
{
    public static bool Prefix(NCreature __instance)
    {
        if (!MiyakoAnimationDriver.IsSpriteDriven(__instance))
            return true;

        MiyakoAnimationDriver.PlayIdle(__instance);
        return false;
    }
}

// Patch 3: Intercept Merchant _Ready → play relaxed animation
[HarmonyPatch(typeof(NMerchantCharacter), "_Ready")]
public static class MiyakoMerchantReadyPatch
{
    public static bool Prefix(NMerchantCharacter __instance)
    {
        if (!MiyakoAnimationDriver.IsSpriteDriven(__instance))
            return true;

        MiyakoAnimationDriver.PlayMerchantRelaxed(__instance);
        return false;
    }
}

// Patch 4: Intercept RestSite _Ready → play act-appropriate loop
[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
public static class MiyakoRestSiteReadyPatch
{
    public static void Postfix(NRestSiteCharacter __instance)
    {
        if (!MiyakoAnimationDriver.IsSpriteDriven(__instance))
            return;

        RunState? runState = Traverse.Create(__instance).Property<RunState>("RunState")?.Value;
        if (runState == null)
            return;

        MiyakoAnimationDriver.PlayRestSiteLoop(__instance, runState.CurrentActIndex);
    }
}
