using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public class DroneDeploy : CustomCardModel
{
    // 核心修改：2费 → 1费
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromOrb<FlashScoutDrone>()];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public DroneDeploy() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 全体群攻伤害（保留原伤害）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);

        // 获得闪光无人机（保留）
        await OrbCmd.Channel<FlashScoutDrone>(choiceContext, Owner);

        // 生成【无人机突袭】（同步升级）
        var assaultCard = CombatState!.CreateCard<DroneAssault>(Owner);
        if (IsUpgraded) CardCmd.Upgrade(assaultCard);
        // 🔥 仅修复：替换旧参数为官方原版写法
        await CardPileCmd.AddGeneratedCardToCombat(assaultCard, PileType.Hand, Owner, CardPilePosition.Random);

        // 新增：生成【无人机同频】（同步升级）到手牌
        var syncCard = CombatState!.CreateCard<DroneSync>(Owner);
        if (IsUpgraded) CardCmd.Upgrade(syncCard);
        // 🔥 仅修复：替换旧参数为官方原版写法
        await CardPileCmd.AddGeneratedCardToCombat(syncCard, PileType.Hand, Owner, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        // 原伤害升级
        DynamicVars.Damage.UpgradeValueBy(2m);
        AddKeyword(CardKeyword.Innate);
    }
}