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

// 卡池
[Pool(typeof(MiyakoCardPool))]
public class ShortBurst : CustomCardModel
{
    // 0费
    private const int energyCost = 0;
    // 攻击牌
    private const CardType type = CardType.Attack;
    // 稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 单体目标
    private const TargetType targetType = TargetType.AnyEnemy;
    // 图鉴显示
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { MyKeywords.Smg };
    // 1星费用
    public override int CanonicalStarCost => 1;

    // 动态变量：基础2点伤害，连击3次 (2×3)
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(3)
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public ShortBurst() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 单体连击伤害（完全参照模板）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害2→3 (3×3)，连击次数不变
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}