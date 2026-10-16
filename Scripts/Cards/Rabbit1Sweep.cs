using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// RABBIT1，开始清扫：衍生攻击牌，只能由「兔子的獠牙」生成。
/// 伤害 = 50 + 自身已损失生命 × 50%（越惨越痛）。
///
/// 数值构成照原版 BodySlam 的写法：
/// CalculatedDamage = CalculationBase + ExtraDamage × 乘数 = 50 + 0.5 × 已损失生命。
/// 只用一条 CalculatedDamageVar，同时负责"结算读数"与"卡面显示"，不再另开 DamageVar。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class Rabbit1Sweep : CustomCardModel
{
    // 3费 | 攻击 | 衍生（Token）| 单体
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(50m),                 // 固定 50
        new ExtraDamageVar(0.5m),                    // 每 1 点已损失生命 → 0.5 伤害
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            card.Owner.Creature.MaxHp - card.Owner.Creature.CurrentHp)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Rabbit1Sweep() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(10m);  // 50 → 60
        DynamicVars.ExtraDamage.UpgradeValueBy(0.1m);     // 50% → 60%
    }
}
