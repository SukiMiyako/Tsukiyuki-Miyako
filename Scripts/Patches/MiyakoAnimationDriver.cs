using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

#nullable disable

namespace TsukiyukiMiyako.Scripts.Patches;

/// <summary>
/// Drives AnimatedSprite2D animations for Miyako's custom character visuals.
/// The game's default Spine2D animation system cannot drive AnimatedSprite2D nodes,
/// so this class translates NCreature animation triggers into AnimatedSprite2D.Play() calls.
/// </summary>
internal static class MiyakoAnimationDriver
{
    /// <summary>
    /// Check whether a creature's visuals are driven by AnimatedSprite2D (not Spine2D).
    /// </summary>
    public static bool IsSpriteDriven(NCreature creature)
    {
        return GodotObject.IsInstanceValid(creature.Visuals)
            && TryGetAnimatedSprite(creature.Visuals, out _);
    }

    /// <summary>
    /// Check whether a node tree contains an AnimatedSprite2D.
    /// </summary>
    public static bool IsSpriteDriven(Node node)
    {
        return TryGetAnimatedSprite(node, out _);
    }

    /// <summary>
    /// Translate a game animation trigger into an AnimatedSprite2D animation name and play it.
    /// </summary>
    public static void Trigger(NCreature creature, string trigger)
    {
        if (!GodotObject.IsInstanceValid(creature.Visuals))
            return;

        if (!TryGetAnimatedSprite(creature.Visuals, out AnimatedSprite2D sprite) || sprite.SpriteFrames == null)
            return;

        string animName = ResolveCombatAnimationName(sprite, trigger);
        if (animName != null)
        {
            Play(sprite, animName, !string.Equals(trigger, "Dead", System.StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Scan a room node's children for AnimatedSprite2D-driven characters and trigger a named animation.
    /// </summary>
    public static void TriggerOnRoomCharacters(Node room, string preferredAnimation)
    {
        foreach (Node child in room.GetChildren(false))
        {
            if (TryGetAnimatedSprite(child, out AnimatedSprite2D sprite) && sprite.SpriteFrames != null)
            {
                string animName = FindFirstAnimation(sprite, preferredAnimation, "idle_loop", "idle");
                if (animName != null)
                {
                    sprite.Play(animName, 1f, false);
                }
            }
        }
    }

    /// <summary>
    /// Trigger die animations for all players on the game over screen.
    /// Creates fresh NCreatureVisuals for each player (bypassing _Ready Spine errors),
    /// then plays die/hurt on any AnimatedSprite2D-driven visuals.
    /// </summary>
    public static void TriggerDieOnScreen(Node screen)
    {
        Log.Warn("[Miyako] TriggerDieOnScreen called");

        // Get all players from the game over screen's run state
        var runState = Traverse.Create(screen).Field("_runState").GetValue();
        if (runState == null)
        {
            Log.Warn("[Miyako] _runState is null");
            return;
        }

        // Access Players via Traverse since the type might be internal
        var players = Traverse.Create(runState).Property("Players").GetValue() as System.Collections.IList;
        if (players == null || players.Count == 0)
        {
            Log.Warn("[Miyako] No players found");
            return;
        }

        Control creatureContainer = Traverse.Create(screen).Field<Control>("_creatureContainer").Value;
        if (creatureContainer == null)
        {
            Log.Warn("[Miyako] _creatureContainer is null");
            return;
        }

        Log.Warn($"[Miyako] Processing {players.Count} players for death animation");
        foreach (var playerObj in players)
        {
            var creature = Traverse.Create(playerObj).Property("Creature").GetValue();
            if (creature == null)
            {
                Log.Warn("[Miyako] Player has null Creature");
                continue;
            }

            // Call CreateVisuals() — this instantiates the TSCN scene directly,
            // bypassing _Ready() which would fail with Spine errors
            var visuals = Traverse.Create(creature).Method("CreateVisuals").GetValue() as NCreatureVisuals;
            if (visuals == null)
            {
                Log.Warn("[Miyako] CreateVisuals returned null");
                continue;
            }

            // Add to container so it renders
            creatureContainer.AddChildSafely(visuals);

            // Check if this is AnimatedSprite2D-driven
            if (TryGetAnimatedSprite(visuals, out AnimatedSprite2D sprite) && sprite.SpriteFrames != null)
            {
                string animName = FindFirstAnimation(sprite, "die", "hurt", "idle_loop", "idle");
                if (animName != null)
                {
                    Log.Warn($"[Miyako] Playing '{animName}' death animation");
                    sprite.Play(animName, 1f, false);
                }
                else
                {
                    Log.Warn("[Miyako] No die/hurt/idle animation found on visual");
                }
            }
            else
            {
                Log.Warn("[Miyako] Creature visuals are not AnimatedSprite2D, freeing");
                visuals.QueueFreeSafely();
            }
        }
    }

    /// <summary>
    /// Play the idle animation on a creature.
    /// </summary>
    public static void PlayIdle(NCreature creature)
    {
        if (!GodotObject.IsInstanceValid(creature.Visuals))
            return;

        if (!TryGetAnimatedSprite(creature.Visuals, out AnimatedSprite2D sprite) || sprite.SpriteFrames == null)
            return;

        string animName = FindFirstAnimation(sprite, "idle_loop", "idle");
        if (animName != null)
        {
            Play(sprite, animName);
        }
    }

    /// <summary>
    /// Play the relaxed animation for merchant scenes.
    /// </summary>
    public static void PlayMerchantRelaxed(Node node)
    {
        if (!TryGetAnimatedSprite(node, out AnimatedSprite2D sprite))
            return;

        string animName = FindFirstAnimation(sprite, "relaxed_loop", "idle_loop", "idle");
        if (animName != null)
        {
            Play(sprite, animName);
        }
    }

    /// <summary>
    /// Play a merchant animation by name, with fallback to relaxed/idle.
    /// </summary>
    public static void PlayMerchantAnimation(Node node, string requestedAnimation)
    {
        if (!TryGetAnimatedSprite(node, out AnimatedSprite2D sprite))
            return;

        string animName = FindFirstAnimation(sprite, requestedAnimation, "relaxed_loop", "idle_loop", "idle");
        if (animName != null)
        {
            Play(sprite, animName);
        }
    }

    /// <summary>
    /// Play the act-appropriate loop animation for rest site scenes.
    /// </summary>
    public static void PlayRestSiteLoop(Node node, int currentActIndex)
    {
        if (!TryGetAnimatedSprite(node, out AnimatedSprite2D sprite))
            return;

        string animName = currentActIndex switch
        {
            0 => FindFirstAnimation(sprite, "overgrowth_loop", "idle_loop", "idle"),
            1 => FindFirstAnimation(sprite, "hive_loop", "idle_loop", "idle"),
            2 => FindFirstAnimation(sprite, "glory_loop", "idle_loop", "idle"),
            _ => FindFirstAnimation(sprite, "idle_loop", "idle"),
        };

        if (animName != null)
        {
            Play(sprite, animName);
        }
    }

    /// <summary>
    /// Attempt to find an AnimatedSprite2D in the node tree.
    /// Looks for a child named "Visuals" first, then falls back to any AnimatedSprite2D descendant.
    /// </summary>
    public static bool TryGetAnimatedSprite(Node node, out AnimatedSprite2D sprite)
    {
        sprite = null;

        Node visualsChild = node.FindChild("Visuals", true, false);
        if (visualsChild is AnimatedSprite2D as2D)
        {
            sprite = as2D;
            return true;
        }

        sprite = node.GetChildren(false).OfType<AnimatedSprite2D>().FirstOrDefault();
        return sprite != null;
    }

    /// <summary>
    /// Play an animation on the sprite. Non-looping animations automatically return to idle when finished.
    /// </summary>
    private static void Play(AnimatedSprite2D sprite, string animation, bool returnToIdle = true)
    {
        sprite.Play(animation, 1f, false);

        if (returnToIdle && !IsLoopingAnimation(animation))
        {
            TaskHelper.RunSafely(ReturnToIdleAfterFinish(sprite, animation));
        }
    }

    /// <summary>
    /// Wait for a non-looping animation to finish, then return to idle.
    /// </summary>
    private static async Task ReturnToIdleAfterFinish(AnimatedSprite2D sprite, string animation)
    {
        await sprite.ToSignal(sprite, AnimatedSprite2D.SignalName.AnimationFinished);

        if (!GodotObject.IsInstanceValid(sprite) || sprite.SpriteFrames == null)
            return;

        if (string.Equals(sprite.Animation.ToString(), animation, System.StringComparison.Ordinal))
        {
            string idle = FindFirstAnimation(sprite, "idle_loop", "idle", "relaxed_loop");
            if (idle != null)
            {
                sprite.Play(idle, 1f, false);
            }
        }
    }

    /// <summary>
    /// Map a game animation trigger word to the best available SpriteFrames animation.
    /// </summary>
    private static string ResolveCombatAnimationName(AnimatedSprite2D sprite, string trigger)
    {
        return trigger switch
        {
            "Idle" => FindFirstAnimation(sprite, "idle_loop", "idle"),
            "Attack" => FindFirstAnimation(sprite, "attack", "cast", "idle_loop", "idle"),
            "Cast" => FindFirstAnimation(sprite, "cast", "attack", "idle_loop", "idle"),
            "Hit" => FindFirstAnimation(sprite, "hurt", "idle_loop", "idle"),
            "Dead" => FindFirstAnimation(sprite, "die", "hurt", "idle_loop", "idle"),
            "Relaxed" => FindFirstAnimation(sprite, "relaxed_loop", "idle_loop", "idle"),
            "Revive" => FindFirstAnimation(sprite, "idle_loop", "idle"),
            _ => null,
        };
    }

    /// <summary>
    /// Find the first available animation from a prioritized list of names.
    /// </summary>
    private static string FindFirstAnimation(AnimatedSprite2D sprite, params string[] names)
    {
        SpriteFrames frames = sprite.SpriteFrames;
        if (frames == null)
            return null;

        foreach (string name in names)
        {
            if (frames.HasAnimation(name))
                return name;
        }

        return null;
    }

    /// <summary>
    /// Check whether an animation name should loop (i.e. it's an idle/relaxed variant).
    /// </summary>
    private static bool IsLoopingAnimation(string animation)
    {
        return string.Equals(animation, "idle_loop", System.StringComparison.Ordinal)
            || string.Equals(animation, "idle", System.StringComparison.Ordinal)
            || string.Equals(animation, "relaxed_loop", System.StringComparison.Ordinal)
            || string.Equals(animation, "overgrowth_loop", System.StringComparison.Ordinal)
            || string.Equals(animation, "hive_loop", System.StringComparison.Ordinal)
            || string.Equals(animation, "glory_loop", System.StringComparison.Ordinal);
    }
}
