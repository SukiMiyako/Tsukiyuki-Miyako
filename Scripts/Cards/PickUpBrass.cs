using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「捡弹壳」——0 能量 / 技能 / 普通 / 自身。
///
/// **消耗 1 张手牌**，获得 1 点能量（升级 2 点）。
///
/// 定位：前三场"回费荒"的补丁。池子里给能量的牌全是 Uncommon/Rare
/// （空投补给、高空突袭、轻装上阵、光明正大的偷看、兼职），Common 档一张都没有，
/// 而她的牌又偏 1 费，每回合 3 点能量只够打两张 —— 这张牌就是让"手牌里的废纸"变成费用。
///
/// 和「重新整备」是同一条消耗体系的两端：
///   · 重新整备：整只手牌换 4 张配备（大换血）
///   · 捡弹壳：一张牌换 1 点能量（零钱兑换）
/// 走 `CardCmd.Exhaust` 逐张消耗，所以「省着点用」这类"每有一张牌被消耗就抽 1"的钩子会正常触发。
///
/// 手牌为空时按"没有可消耗的牌"处理，**不会白给能量** —— 卡面写的就是"烧一张换一点"。
///
/// 关键词：不带消耗（它自己是一次性的工具，但没必要再标 Exhaust）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class PickUpBrass : CustomCardModel
{
    // 0费 | 技能 | 普通 | 自身
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public PickUpBrass() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 选一张手牌烧掉（照原版 BurningPact 的写法）
        CardModel? picked = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: Owner,
            prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            filter: null,
            source: this)).FirstOrDefault();

        if (picked == null)
        {
            // 手里没牌 → 什么都不发生，不白给能量
            return;
        }

        await CardCmd.Exhaust(choiceContext, picked);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        // 1 点 → 2 点能量
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
