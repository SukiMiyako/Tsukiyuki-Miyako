using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class RainyStand : CustomCardModel
{
    // 配置：3费 | 能力牌 | 稀有 | 自身目标
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 核心变量
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("RainyStand", 1m)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public RainyStand() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 施法逻辑 1:1 照搬原版
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<RainyStandPower>(Owner.Creature, DynamicVars["RainyStand"].BaseValue, Owner.Creature, this);
    }

    // 升级：3费 → 2费（费用-1）
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}