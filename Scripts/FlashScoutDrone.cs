using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TsukiyukiMiyako.Scripts;

public class FlashScoutDrone : CustomOrbModel
{
    //基础数值
    public override decimal PassiveVal => ModifyOrbValue(3m);
    public override decimal EvokeVal => ModifyOrbValue(6m);
    //格挡数值（吃集中）
    private decimal BlockVal => ModifyOrbValue(2m);

    public override Color DarkenedColor => new(0.1f, 0.2f, 0.5f);
    public override string? CustomIconPath => "res://icon.svg";

    public override Node2D? CreateCustomSprite()
    {
        return PreloadManager.Cache.GetScene("res://Tsukiyuki Miyako/scenes/orb.tscn").Instantiate<Node2D>();
    }

    //回合结束触发（官方统一格式）
    public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    //被动：随机单体伤害 + 格挡（冰球格式）
    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();

        //单体伤害（官方逻辑）
        List<Creature> enemies = CombatState.HittableEnemies.Where(e => e.IsHittable).ToList();
        if (enemies.Any())
        {
            Creature randomTarget = Owner.RunState.Rng.CombatTargets.NextItem(enemies)!;
            await CreatureCmd.Damage(choiceContext, new[] { randomTarget }, PassiveVal, ValueProp.Unpowered, Owner.Creature);
        }

        //格挡 → 完全照搬 FrostOrb
        await CreatureCmd.GainBlock(Owner.Creature, BlockVal, ValueProp.Unpowered, null);
    }

    //激发：全体伤害 → 完全照搬 GlassOrb
    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        List<Creature> enemies = CombatState.HittableEnemies.Where(e => e.IsHittable).ToList();

        if (enemies.Any())
        {
            await CreatureCmd.Damage(choiceContext, enemies, EvokeVal, ValueProp.Unpowered, Owner.Creature);
        }

        return enemies;
    }
}