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
using TsukiyukiMiyako.Scripts;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「离我远点！」——2 能量 / 攻击 / 普通 / 单体。**仅能在[寂寞]状态下打出，消耗。**
///
/// 造成 15 点伤害，并获得 1 层[gold]缓冲[/gold]。
///
/// 定位：寂寞里的"续航件"。寂寞期敏捷被削到几乎没格挡（−8 时九成格挡牌归 0），
/// 所以防御手段改成[gold]缓冲[/gold] —— 原版 <see cref="BufferPower"/> 是
/// `ModifyHpLostAfterOstyLate` 把这一下掉血归 0 然后减 1 层，
/// **在格挡结算之后才判定**，所以残余格挡吃小伤害、缓冲只兜穿透进来的大伤害。
///
/// 对多段敌人三层两下就烧完，对 Boss 的单发重击等于完全豁免一次 —— 强度上限被敌人形态卡住。
///
/// 关键词：
///   · 消耗（项目规则 —— 只可在[寂寞]下打出的牌一律带消耗）
///   · RABBIT —— 寂寞专属，且**寂寞结束时缓冲一起收回**（不给白嫖）。
///     收回由 <see cref="TsukiyukiMiyako.Scripts.Powers.StayAwayPower"/> 负责：
///     卡牌当场给缓冲，power 只负责记账，寂寞结束时把没被打掉的那部分扣回来。
///   不带 SMG：这张牌**不花弹夹**，按池子口径（花弹夹的牌才是 SMG）不给。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class StayAway : CustomCardModel
{
    // 2费 | 攻击 | 普通 | 单体
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>消耗 —— 项目规则：只可在[寂寞]下打出的牌一律带消耗。</summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        MyKeywords.Rabbit
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new PowerVar<BufferPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BufferPower>(),
        HoverTipFactory.FromPower<LonelinessPower>(),
        HoverTipFactory.FromPower<StayAwayPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public StayAway() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>
    /// 仅在寂寞（羁绊为 0 或负数）状态下可打出 —— 照「兔子的獠牙」的写法。
    /// 用 `IsPlayable` 而不是 `ShouldPlay`：后者是给监听者拦牌用的，
    /// 用它会让卡牌在手里被渲染成"能打但打不出"的状态。
    /// </summary>
    protected override bool IsPlayable =>
        Owner != null
        && Owner.Creature.GetPowerAmount<LonelinessPower>() > 0;

    /// <summary>满足条件时金光高亮（原版 GrandFinale 的写法）。</summary>
    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1) 先打伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 2) 再上一层缓冲（只对之后的掉血生效，所以顺序无所谓，放后面更直观）
        decimal buffer = DynamicVars["BufferPower"].BaseValue;
        await PowerCmd.Apply<BufferPower>(
            choiceContext,
            Owner.Creature,
            buffer,
            Owner.Creature,
            this);

        // 3) 挂上记账用的 power：寂寞结束时把没被打掉的缓冲收回去（RABBIT 铁律）
        await PowerCmd.Apply<StayAwayPower>(
            choiceContext,
            Owner.Creature,
            buffer,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 升级只加伤害：缓冲层数不动（多一层缓冲约等于多一次完整豁免，太贵）
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
