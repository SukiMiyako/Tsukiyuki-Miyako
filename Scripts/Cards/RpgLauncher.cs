using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 标枪火箭弹（RPG）：单段重击 + 相邻溅射。
///
/// 为什么要单段：蜂群术士（Entomancer）的 `Personal Hive` 按"被命中次数"塞 Dazed，
/// 多段牌会被重罚，所以主目标走单段高伤。
///
/// 实现要点：
/// - 基础游戏没有"相邻"这个 TargetType，所以相邻目标靠 `CombatState.Enemies` 的站位顺序自己算。
/// - `AttackCommand.Targeting` 只接受单个目标，相邻（可能多个）用 `CreatureCmd.Damage(targets)` 一次结算。
/// - 单怪战斗跳过溅射，否则主目标会吃到两份伤害。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class RpgLauncher : CustomCardModel
{
    // 3费 | 攻击 | 罕见 | 单体（相邻由站位推导）
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14m, ValueProp.Move),           // 主目标
        new DynamicVar("Splash", 5m)                  // 相邻溅射
    ];

    // 配备体系：可被军备箱生成、消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        MyKeywords.Equipment
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public RpgLauncher() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        var main = cardPlay.Target!;

        // 发射音（用户录的 RPG.mp3）
        SfxCmd.Play("res://Audio/RPG.mp3");

        // 1) 【必须最先做】先把相邻目标锁下来。
        // 一旦主目标被击杀，它会从 CombatState.Enemies 里消失/导致站位重排，
        // 之后再按"主目标的左右"去取相邻就会错位甚至取空。
        var splashTargets = GetAdjacentTargets(main);

        // 2) 主目标：单段高伤（可能直接击杀）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(main)
            .Execute(choiceContext);

        // 3) 相邻溅射：用第 1 步锁定的列表结算（已死的目标由伤害系统自行跳过）
        if (splashTargets.Count > 0)
        {
            await CreatureCmd.Damage(
                choiceContext,
                splashTargets,
                base.DynamicVars["Splash"].BaseValue,
                ValueProp.Move,
                base.Owner.Creature,
                this);
        }
    }

    /// <summary>取主目标左右相邻的敌人（存活可命中的）。单怪战斗返回空。</summary>
    private List<Creature> GetAdjacentTargets(Creature main)
    {
        var enemies = base.CombatState!.Enemies;
        var result = new List<Creature>();

        int index = -1;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == main)
            {
                index = i;
                break;
            }
        }
        if (index < 0)
        {
            return result;
        }

        if (index - 1 >= 0 && enemies[index - 1].IsHittable)
        {
            result.Add(enemies[index - 1]);
        }
        if (index + 1 < enemies.Count && enemies[index + 1].IsHittable)
        {
            result.Add(enemies[index + 1]);
        }
        return result;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);            // 14 → 18
        DynamicVars["Splash"].UpgradeValueBy(2m);         // 5 → 7
    }
}
