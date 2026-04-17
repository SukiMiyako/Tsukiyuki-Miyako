using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace TsukiyukiMiyako.Scripts.Cards;

// 加入哪个卡池
[Pool(typeof(MiyakoCardPool))]
public class C4 : CustomCardModel
{
    // 基础耗能
    private const int energyCost = 0;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust,
        MyKeywords.Equipment
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public C4() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CustomCardModel Expoluted = CombatState!.CreateCard<Expoluted>(Owner);
        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(Expoluted);
        }

        // 【仅添加内核加速同款动画，无其他修改】
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(Expoluted, PileType.Draw, true));
        await Cmd.Wait(0.5f);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}