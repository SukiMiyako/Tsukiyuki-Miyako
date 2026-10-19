using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Resources;

namespace TsukiyukiMiyako.Scripts.Cards;

// 加入哪个卡池
[Pool(typeof(MiyakoCardPool))]
public class FireStorm : CustomCardModel
{
    // 基础耗能
    private const int energyCost = 1;
    // X 费弹夹（自有资源 MagazineResource，不再用储君的星）：
    // 打出时把手里所有弹夹都算作 X，连击数 = X*2
    private const bool shouldShowInCardLibrary = true;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.RandomEnemy;
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
    };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { MyKeywords.Smg };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public FireStorm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        // X 费弹夹：花费 = 当前全部弹夹（BaseLib 会记录本张牌实际花掉多少）
        CustomResources<MagazineResource>.SetXCost(this);
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SfxCmd.Play("res://Audio/firestorm.ogg");
        // 核心攻击：3点伤害 × X*2 次连击，随机敌人
        // X = 这张牌实际花掉的弹夹（CustomResources.AmountSpent，BaseLib 在扣费后写入）
        int spent = CustomResources<MagazineResource>.AmountSpent(cardPlay);
        if (spent > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(spent * 2) // 连击次数
                .FromCardCompatibility(this, cardPlay)
                .TargetingRandomOpponents(base.CombatState!)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1); // 升级后增加3点伤害
    }

}