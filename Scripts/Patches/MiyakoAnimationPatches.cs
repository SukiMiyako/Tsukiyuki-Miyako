using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

namespace TsukiyukiMiyako.Scripts.Patches;

/// <summary>
/// Harmony patches for Miyako's AnimatedSprite2D-driven character visuals.
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

// Patch 3: GameOver screen — trigger die animation on AnimatedSprite2D creatures
[HarmonyPatch(typeof(NGameOverScreen), "AfterOverlayOpened")]
public static class MiyakoGameOverDiePatch
{
    public static void Postfix(NGameOverScreen __instance)
    {
        MiyakoAnimationDriver.TriggerDieOnScreen(__instance);
    }
}
