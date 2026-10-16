using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 兔子的獠牙：绝望中的爆发。
/// 仅在「寂寞」状态下可打出：失去生命换巨额力量，代价是脆弱与敏捷崩坏。
///
/// 加伤（力量）与减防（敏捷）绑定在「寂寞」上：打出去会挂上
/// <see cref="RabbitFangPower"/> 状态图标，寂寞一结束就一起撤销；
/// 只有脆弱留在身上 —— 那是她用力过的证据，也是这张牌真正的代价。
///
/// 状态期间不可重复打出：两次獠牙会让"该撤销多少力量/敏捷"变成一笔烂账，
/// 所以 IsPlayable 里顺手排除了已处于獠牙状态的情况。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class RabbitFang : CustomCardModel
{
    // 0费 | 技能 | 稀有 | 自身
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>「脆弱」持续回合数（99 ≈ 整场战斗）。</summary>
    private const decimal FrailTurns = 99m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(10m),                        // 失去生命（不吃格挡/减伤）
        new PowerVar<StrengthPower>(10m),          // 获得力量
        new PowerVar<FrailPower>(FrailTurns),      // 脆弱
        new DynamicVar("DexLoss", RabbitFangPower.DexterityPenalty)   // 失去敏捷
    ];

    /// <summary>
    /// 消耗 —— 项目规则：**只可在[寂寞]下打出的牌一律带消耗**
    /// （这类牌都是"绝望里的爆发"，不该反复用）。
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<RabbitFangPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<FrailPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public RabbitFang() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>
    /// 仅在寂寞（羁绊为 0 或负数）状态下可打出，且不能重复叠加獠牙状态。
    /// 用 `IsPlayable` 而不是 `ShouldPlay` —— 前者是卡牌自身"能否打出"的判定（原版 GrandFinale 的写法），
    /// 后者是给监听者拦牌用的，用它会导致卡牌在手里被渲染成不可打出。
    /// </summary>
    protected override bool IsPlayable =>
        Owner != null
        && Owner.Creature.GetPowerAmount<LonelinessPower>() > 0
        && Owner.Creature.GetPowerAmount<RabbitFangPower>() == 0;

    /// <summary>满足条件时金光高亮（原版 GrandFinale 的写法）。</summary>
    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1) 失去生命（Unblockable | Unpowered | Move = 官方 cardHpLoss，不吃格挡与减伤）
        await CreatureCmd.Damage(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this);

        // 2) 获得力量
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["StrengthPower"].BaseValue,
            base.Owner.Creature,
            this);

        // 3) 脆弱（格挡收益 ×0.75）
        await PowerCmd.Apply<FrailPower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["FrailPower"].BaseValue,
            base.Owner.Creature,
            this);

        // 4) 失去敏捷（负数额度；DexterityPower 允许为负）
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            base.Owner.Creature,
            -base.DynamicVars["DexLoss"].BaseValue,
            base.Owner.Creature,
            this);

        // 5) 挂上「獠牙」状态图标（层数 = 这一口拿到的力量）
        //    寂寞结束时，由 RabbitFangPower 用它把加伤与减防一并撤销
        await PowerCmd.Apply<RabbitFangPower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["StrengthPower"].BaseValue,
            base.Owner.Creature,
            this);

        // 6) 生成「RABBIT1，开始清扫」到手牌（随本牌的升级状态同步升级）
        var sweep = base.CombatState!.CreateCard<Rabbit1Sweep>(base.Owner);
        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(sweep);
        }
        await CardPileCmd.AddGeneratedCardToCombat(sweep, PileType.Hand, base.Owner, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        // 升级只加强收益（力量 10 → 14），负面效果（失血/脆弱/敏降）不减
        DynamicVars["StrengthPower"].UpgradeValueBy(4m);
    }
}
