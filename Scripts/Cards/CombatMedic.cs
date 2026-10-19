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
/// 战地医疗处理：1费 / 技能 / 普通 / 自身 / **消耗**。
///
/// 把手牌中所有[状态]牌转化为[gold]绷带[/gold]（0 费、回血并抽牌）。
/// **升级：去掉「消耗」**（费用不变，仍是 1 费）。
///
/// 定位：状态污染的即时解 —— 专门对付「沙漏」这类"每打若干张牌就往手里塞
/// 不可打出的[gold]凋萎[/gold]"的敌人。凋萎是不可打出 + 回合结束自伤，
/// 留在手里既是废牌又在扣血，换成绷带是双重收益。
///
/// 过滤器写 `IsTransformable &amp;&amp; Type == CardType.Status`：
/// `IsTransformable` 只看有没有[gold]永恒[/gold]关键词，凋萎没有，所以能转。
/// 用 `CardCmd.Transform` 走官方转化流程（不是"消耗 + 生成"），
/// 状态牌自身的移除钩子与动画才能正常走。
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

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

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

    protected override void OnUpgrade()
    {
        // 升级：不再消耗（费用维持 1 费）
        RemoveKeyword(CardKeyword.Exhaust);
    }
}