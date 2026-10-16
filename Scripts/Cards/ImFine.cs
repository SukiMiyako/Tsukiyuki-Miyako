using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「我……我没事」——1费 / 技能 / 罕见 / 自身。**仅能在[寂寞]状态下打出。**
///
/// 失去 2 点生命，获得 +3 力量、+3 敏捷；**寂寞一结束，加成都一起消失**。
///
/// 和「兔子的獠牙」同一条线（寂寞里的爆发），但方向相反：
///   獠牙是"换一口大打"，这张是"咬紧牙关把状态拉回来"——
///   寂寞本身带 -1 力量/-1 敏捷（LonelinessPower 的 debuff），这张正好把窟窿补上一截。
///   代价是失血，而她的 RABBIT1 又是按"已损失生命"放大的，两者刚好互喂。
///
/// 状态收回由 <see cref="ImFinePower"/> 负责（寂寞结束时把力量与敏捷一起撤掉）。
/// 状态期间不可重复打出：两次叠上去之后"该收回多少"会变成烂账（和獠牙同一条规则）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class ImFine : CustomCardModel
{
    // 1费 | 技能 | 罕见 | 自身
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(2m),                        // 失去生命（不吃格挡/减伤）
        new PowerVar<StrengthPower>(3m),          // +力量
        new PowerVar<DexterityPower>(3m)          // +敏捷
    ];

    /// <summary>消耗 —— 项目规则：只可在[寂寞]下打出的牌一律带消耗。</summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ImFinePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public ImFine() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>仅在寂寞中、且尚未处于本状态时可打出。</summary>
    protected override bool IsPlayable =>
        Owner != null
        && Owner.Creature.GetPowerAmount<LonelinessPower>() > 0
        && Owner.Creature.GetPowerAmount<ImFinePower>() == 0;

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

        // 2) +力量
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["StrengthPower"].BaseValue,
            base.Owner.Creature,
            this);

        // 3) +敏捷
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["DexterityPower"].BaseValue,
            base.Owner.Creature,
            this);

        // 4) 状态图标（层数 = 数值；寂寞结束时由它把两项一起收回）
        await PowerCmd.Apply<ImFinePower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["StrengthPower"].BaseValue,
            base.Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 力量与敏捷各 +1（3 → 4），失血不变 —— 代价不跟着升级
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
        DynamicVars["DexterityPower"].UpgradeValueBy(1m);
    }
}
