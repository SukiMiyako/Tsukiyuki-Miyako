using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class Fatigue : CustomCardModel
{
    // 每回合最大出牌限制：5张
    private const int MaxCardsPerTurn = 5;
    private const string CalculatedCardsKey = "CalculatedCards";

    // 超出限制时红色高亮
    protected override bool ShouldGlowRedInternal => ShouldPreventCardPlay;

    // 核心逻辑：本回合打出≥5张牌时，禁止继续出牌
    private bool ShouldPreventCardPlay => CardsPlayedThisTurn >= MaxCardsPerTurn;
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        CardKeyword.Ethereal
    ];

    // 状态牌无升级
    public override int MaxUpgradeLevel => 0;

    /// <summary>
    /// 不能被"随机生成"类效果抽到。
    ///
    /// 原版的状态牌（伤口 / 灼伤 / 眩晕……）挂在 `StatusCardPool` 里，而我们在
    /// `[Pool(typeof(MiyakoCardPool))]`——天然混在**角色的可玩牌池**里。
    /// `CardFactory.FilterForCombat` 只排除 Basic / Ancient / Event 三档稀有度，
    /// `CardRarity.Status` 拦不住，于是没按类型过滤的生成效果（发现 / 选择悖论 /
    /// 疯狂科学 / 泼溅……）会把状态牌抽进三选一。
    /// 关掉这个开关后所有走 `FilterForCombat` 的路径都会把它筛掉；
    /// 我们自己的产牌（兼职）走的是 `CombatState.CreateCard`，不受影响。
    /// </summary>
    public override bool CanBeGeneratedInCombat => false;

    public Fatigue()
        // 状态牌标准配置：0费 | 状态牌 | 状态稀有度 | 无目标
        : base(-1, CardType.Status, CardRarity.Status, TargetType.None)
    {
    }

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 统计本回合已打出的卡牌数量
    private int CardsPlayedThisTurn => CombatManager.Instance.History.CardPlaysStarted
        .Count(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Owner == Owner);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(8m),
        new CalculationExtraVar(-1m),
        new CalculatedVar(CalculatedCardsKey).WithMultiplier((card, _) => Math.Min(8, ((Fatigue)card).CardsPlayedThisTurn))
    ];

    // 出牌限制核心逻辑
    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner != Owner)
            return true;

        if (Pile?.Type != PileType.Hand)
            return true;

        return !ShouldPreventCardPlay;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
    }
}