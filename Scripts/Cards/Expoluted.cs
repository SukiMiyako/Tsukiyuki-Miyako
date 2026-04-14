using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TsukiyukiMiyako.Scripts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;


namespace TsukiyukiMiyako.Scripts.Cards;


// 加入哪个卡池
[Pool(typeof(MiyakoCardPool))]
public class Expoluted : CustomCardModel
{
    // 基础耗能
    private const int energyCost = 3;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Token;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AllAllies;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move)];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";


    public static async Task<CardModel?> CreateInHand(Player owner, CombatState combatState)
    {
        return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
    }
    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, CombatState combatState)
    {
        // 1. 如果要生成的数量是 0 → 直接返回空，不干活
        if (count == 0)
        {
            return Array.Empty<CardModel>();
        }

        // 2. 如果战斗已经结束/正在结束 → 不生成牌
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return Array.Empty<CardModel>();
        }

        // 3. 创建一个列表，用来存放即将生成的匕首牌
        List<CardModel> Expoluted = new List<CardModel>();

        // 4. 循环 count 次：生成对应数量的匕首牌
        for (int i = 0; i < count; i++)
        {
            // 用当前战斗状态，创建一张 Shiv 匕首牌，归属给玩家
            Expoluted.Add(combatState.CreateCard<C4>(owner));
        }

        // 5. 把所有生成的匕首牌 → 加入玩家【手牌】
        await CardPileCmd.AddGeneratedCardsToCombat(Expoluted, PileType.Draw, addedByPlayer: true);

        // 6. 返回生成好的所有匕首牌
        return Expoluted;
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust,
    };
    public Expoluted() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue) // 造成伤害，数值来源于卡牌的基础伤害属性
            .FromCard(this) // 伤害来源于这张卡牌
            .TargetingAllOpponents(base.CombatState!)
            .Execute(choiceContext);
    }


    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4); // 升级后增加5点伤害
        AddKeyword(CardKeyword.Retain);// 升级后保留
    }

}