using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
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
    public override decimal EvokeVal => ModifyOrbValue(_evokeVal);
    //格挡数值（吃集中）
    private decimal BlockVal => 2m;

    // 黑暗球成长数值（初始3，每次被动+3）
    private decimal _evokeVal = 3m;

    public override Color DarkenedColor => new(0.1f, 0.2f, 0.5f);
    public override string? CustomIconPath => "res://flashscoutdrone.svg";

    public override Node2D? CreateCustomSprite()
    {
        return PreloadManager.Cache.GetScene("res://Tsukiyuki Miyako/scenes/orb.tscn").Instantiate<Node2D>();
    }

    //回合结束触发（官方统一格式）
    public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    // =========
    // 原被动：随机伤害+格挡
    // 黑暗球成长机制
    // =========
    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();

        // 1. 随机单体伤害
        List<Creature> enemies = CombatState.HittableEnemies.Where(e => e.IsHittable).ToList();
        if (enemies.Any())
        {
            Creature randomTarget = Owner.RunState.Rng.CombatTargets.NextItem(enemies)!;
            await CreatureCmd.Damage(choiceContext, new[] { randomTarget }, PassiveVal, ValueProp.Unpowered, Owner.Creature);
        }

        // 2. 格挡
        await CreatureCmd.GainBlock(Owner.Creature, BlockVal, ValueProp.Unpowered, null);

        // 3. 黑暗球被动：成长激发伤害（每次+3）
        _evokeVal += 3;
        NCombatRoom.Instance?.GetCreatureNode(Owner.Creature)?.OrbManager?.UpdateVisuals(OrbEvokeType.None);
    }

    // =========
    // 激发：黑暗球逻辑（打最低血量敌人）
    // 删掉了原全体伤害，仅改目标
    // =========
    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        IReadOnlyList<Creature> enemies = CombatState.HittableEnemies;
        if (enemies.Count == 0)
            return Array.Empty<Creature>();

        // 黑暗球逻辑：攻击血量最低的敌人
        Creature weakestEnemy = enemies.MinBy(c => c.CurrentHp)!;
        await CreatureCmd.Damage(choiceContext, weakestEnemy, EvokeVal, ValueProp.Unpowered, Owner.Creature);

        return new[] { weakestEnemy };
    }
}