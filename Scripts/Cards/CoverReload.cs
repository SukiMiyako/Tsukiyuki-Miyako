using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

// 加入哪个卡池
[Pool(typeof(MiyakoCardPool))]
public class CoverReload : CustomCardModel
{
    // 基础耗能 0费
    private const int energyCost = 0;
    // 卡牌类型 技能牌
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型 自身
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 动态变量：3点格挡 + 1星
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(3m, ValueProp.Move),
        new StarsVar(1)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public CoverReload() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        // 获得格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        // 获得星数
        await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级：星数 1→2
        base.DynamicVars.Stars.UpgradeValueBy(1m);
    }

}