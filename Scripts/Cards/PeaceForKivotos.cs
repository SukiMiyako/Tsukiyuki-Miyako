using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class PeaceForKivotos : CustomCardModel
{
    // 核心变量：+2力量（完全照搬燃烧）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(2m)
    ];

    // 悬浮提示（完全照搬燃烧）
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    // 卡牌基础配置：1费 | 能力牌 | 蓝卡(Uncommon) | 自身目标
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 关键词：固有
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public PeaceForKivotos() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 施法效果（1:1 照搬燃烧）
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
    }

    // 升级效果：+2 → +3力量（照搬燃烧）
    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1m);
    }
}