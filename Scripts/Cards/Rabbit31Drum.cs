using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.HoverTips;

namespace TsukiyukiMiyako.Scripts.Cards;

// 加入哪个卡池
[Pool(typeof(MiyakoCardPool))]
public class Rabbit31Drum : CustomCardModel
{
    // 基础耗能 2费
    private const int energyCost = 2;
    // 卡牌类型 技能牌
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型 自身
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    // 动态变量：获得5星
    protected override IEnumerable<DynamicVar> CanonicalVars => [new StarsVar(5)];
    // 消耗关键词提示
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust,
        MyKeywords.Equipment
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Rabbit31Drum() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        // 获得5星能
        await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue, base.Owner);
    }

    // 无升级效果（可按需添加）
    protected override void OnUpgrade()
    {
        DynamicVars.Stars.UpgradeValueBy(1m); 
    }
}