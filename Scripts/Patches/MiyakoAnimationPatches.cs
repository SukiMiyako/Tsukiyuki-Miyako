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

// Patch 1: Intercept SetAnimationTrigger → drive AnimatedSprite2D (combat)
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

// Patch 3: Intercept Merchant PlayAnimation → redirect to AnimatedSprite2D
[HarmonyPatch(typeof(NMerchantCharacter), "PlayAnimation")]
public static class MiyakoMerchantPlayAnimationPatch
{
    public static void Postfix(NMerchantCharacter __instance, string anim)
    {
        if (MiyakoAnimationDriver.IsSpriteDriven(__instance))
        {
            MiyakoAnimationDriver.PlayMerchantAnimation(__instance, anim);
        }
    }
}

// Patch 4: Intercept RestSite OnFocus → kickstart the act-appropriate loop
[HarmonyPatch(typeof(NRestSiteCharacter), "OnFocus")]
public static class MiyakoRestSiteOnFocusPatch
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
