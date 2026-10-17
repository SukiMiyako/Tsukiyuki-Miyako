using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「重新整备」——2 能量 / 技能 / 普通 / 自身。
///
/// **消耗你所有的手牌**，然后获得 4 张随机[gold]配备[/gold]牌。
///
/// 定位：消耗体系的"换弹"件 —— 把手里用不上的东西一次性扔掉，换回四件新装备。
/// 和同族的三张生成牌分工：
///   · 配备箱 1 费换 2 张（免费打出）
///   · 这可比枪好使多了 0 费 + 3 弹夹换 2 张（免费打出）
///   · 重新整备 2 费换 4 张，**代价是全部手牌**（生成的四张免费打出）
/// 所以它的价值不在"生成得多"，而在"把手牌里那些废牌变成资源"。
///
/// 消耗手牌照原版 FiendFire 的写法：**先 ToList() 快照再逐张消耗**。
/// 直接边遍历 `PileType.Hand.GetPile(Owner).Cards` 边 Exhaust 会边改集合。
/// 逐张走 `CardCmd.Exhaust` 也意味着「省着点用」之类的弃牌/消耗钩子会正常触发。
///
/// 结算顺序：先消耗、再生成。反过来的话新的四张会被自己一起消耗掉。
///
/// 升级：**费用 2 → 1**（原版减费写法 `EnergyCost.UpgradeBy(-1)`），
/// 因此升级过的「重新整备」生成出来的配备牌**不跟着升级**——
/// 这张牌的升级项是它自己的费用，跟被生成的牌无关。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class Reequip : CustomCardModel
{
    // 3费 | 技能 | 普通 | 自身
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(4)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Reequip() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1) 消耗所有手牌（先快照，理由见类注释）
        List<CardModel> hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        foreach (CardModel card in hand)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        // 2) 生成 4 张配备，塞回手牌
        IEnumerable<CardModel> forCombat = CardFactory.GetForCombat(
            Owner,
            Owner.Character.CardPool.GetUnlockedCards(
                    Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.CanonicalKeywords.Contains(MyKeywords.Equipment)),
            DynamicVars.Cards.IntValue,
            Owner.RunState.Rng.CombatCardGeneration);

        foreach (CardModel item in forCombat)
        {
            // 配备关键词的规则：被生成的配备本回合内免费打出
            item.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(
                item, PileType.Hand, Owner, CardPilePosition.Random);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：费用 2 → 1（不升级生成出来的牌 —— 见类注释）
        base.EnergyCost.UpgradeBy(-1);
    }
}
