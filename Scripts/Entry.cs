using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// Mod entry point. The initializer name must match the string passed to ModInitializer.
/// </summary>
[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        // Apply Harmony patches for game logic modifications
        // Use a unique Harmony ID to avoid conflicts with other mods
        var harmony = new Harmony("sts2.sh1ro.miyako");
        harmony.PatchAll();

        // Register custom Godot scripts so .tscn scenes can resolve C# types
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        Log.Debug("Mod initialized!");
    }
}
