using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

// 卡池
[Pool(typeof(MiyakoCardPool))]
public class PointBlankShot : CustomCardModel
{
    // 1费
    private const int energyCost = 1;
    // 攻击牌
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 单体目标
    private const TargetType targetType = TargetType.AnyEnemy;
    // 图鉴显示
    private const bool shouldShowInCardLibrary = true;

    // 2星费用
    public override int CanonicalStarCost => 2;

    // 动态变量：基础3点伤害，连击6次 (3×6)
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(6)
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public PointBlankShot() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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

        // 额外效果：给目标施加1层易伤
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target!, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害3→4 (4×6)，连击次数不变
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}