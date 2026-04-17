using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

// 加入你的专属卡池
[Pool(typeof(MiyakoCardPool))]
public class WildShoot : CustomCardModel
{
    // 基础耗能：0费
    private const int energyCost = 0;
    // 卡牌类型：攻击牌
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型：任意敌人
    private const TargetType targetType = TargetType.AnyEnemy;
    // 图鉴显示
    private const bool shouldShowInCardLibrary = true;

    // 1星费用
    public override int CanonicalStarCost => 1;

    // 核心数值：3点伤害，4次连击
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(4)
    };

    // 关键词：SMG
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { MyKeywords.Smg };

    // 标准图标路径
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public WildShoot() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 核心攻击：3点伤害 × 4次连击
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 2. 照搬FightThrough逻辑：将1张【伤口】添加到弃牌堆
        CardModel woundCard = base.CombatState!.CreateCard<Wound>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(woundCard, PileType.Discard, addedByPlayer: true));
    }

    protected override void OnUpgrade()
    {
        // 升级效果：伤害+1（可自行修改）
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}