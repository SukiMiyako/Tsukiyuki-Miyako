using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 战地医疗处理：2费，消耗所有状态牌，转化为绷带
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class CombatMedic : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 悬浮提示：绷带 + 转化
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Bandage>(IsUpgraded),
        HoverTipFactory.Static(StaticHoverTip.Transform)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public CombatMedic() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 施法动画
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // Collect all transformable status cards from hand.
        List<CardModel> statusCards = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c != null && c.IsTransformable && c.Type == CardType.Status)
            .ToList();

        // 遍历转化为绷带
        foreach (CardModel statusCard in statusCards)
        {
            CardModel bandage = CombatState!.CreateCard<Bandage>(Owner);
            // 主卡升级 → 绷带同步升级
            if (IsUpgraded)
            {
                CardCmd.Upgrade(bandage);
            }
            // 转化卡牌
            await CardCmd.Transform(statusCard, bandage);
        }
    }

    // Upgrade: adds Innate keyword.
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}