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
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「标枪飞弹」——1费 / 技能 / 普通 / 单体 / 配备 + 消耗。
///
/// 设计定位（docs/design-notes.md §1.5）：**单段延迟爆发**。
/// 锁一名敌人，下回合开始时发射 —— 延迟本身就是折扣，所以基础伤害比「精准突袭(15)」高一档，
/// 而且是单段：不会被"按被命中次数"惩罚的东西（蜂群术士的 Personal Hive、多层格挡之类）吃掉。
///
/// 实装：OnPlay 只负责"锁定"（往目标身上挂 <see cref="JavelinMissilePower"/>）并播锁定音，
/// 真正的伤害在下回合开始由 power 结算。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class JavelinMissile : CustomCardModel
{
    // 1费 | 技能 | 普通 | 单体
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(17m, ValueProp.Move)
    ];

    // 标枪飞弹是最后一张新增的「配备」牌；之后不再新增配备（现有配备以后整体挪去做 token）。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        MyKeywords.Equipment
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<JavelinMissilePower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public JavelinMissile() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        var target = cardPlay.Target!;

        // 锁定音（打出瞬间）
        SfxCmd.Play("res://Audio/javalin lock.mp3");

        // 把"锁定"挂到目标身上；伤害直接作为 power 的 Amount（提示框才认 {Amount}）
        var power = await PowerCmd.Apply<JavelinMissilePower>(
            choiceContext,
            target,
            base.DynamicVars.Damage.BaseValue,
            base.Owner.Creature,
            this);

        power?.SetSource(this);
        power?.SetSource(this);
    }

    protected override void OnUpgrade()
    {
        // 17 → 21（延迟是折扣，升级空间比即伤牌大一点）
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
