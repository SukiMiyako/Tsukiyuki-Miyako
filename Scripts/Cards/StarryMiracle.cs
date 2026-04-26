using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class StarryMiracle : CustomCardModel
{
    // 配置：1费 | 能力牌(Power) | 稀有 | 自身目标
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 关键词：固有 + 能力牌标识
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Innate
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public StarryMiracle() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 完全照搬子程序的施法逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        // 仅修复：补全官方必填参数
        await PowerCmd.Apply<StarryMiraclePower>(new BlockingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
    }

    // 升级修正：费用-1 → 1费变0费（和子程序完全一致）
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}