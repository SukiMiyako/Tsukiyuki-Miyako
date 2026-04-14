using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 绷带：0费，恢复2点生命，消耗
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class Bandage : CustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    // 回血2点（对标你的回血变量）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(4m),
        new CardsVar(1)
    ];

    // 消耗关键词（和Fuel一致）
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Bandage() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    // 核心：恢复生命值
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.IntValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    // 升级：回血+2 → 2→4
    protected override void OnUpgrade()
    {
    }
}