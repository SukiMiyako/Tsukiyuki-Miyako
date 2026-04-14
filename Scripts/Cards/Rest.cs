using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using TsukiyukiMiyako.Scripts;
using System.Collections.Generic;

namespace TsukiyukiMiyako.Scripts.Cards;

// 加入角色卡池
[Pool(typeof(MiyakoCardPool))]
public class Rest : CustomCardModel
{
    // 基础配置：2费
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    // 基础格挡：13点
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(13m, ValueProp.Move),new EnergyVar(1)];

    // 卡牌图片路径
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Rest() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出效果：动画 + 获得格挡 + 施加力量BUFF + 结束回合
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 触发施法动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 获得格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        // 核心：施加RestPower → 下回合获得2点力量
        await PowerCmd.Apply<RestPower>(base.Owner.Creature, 2, base.Owner.Creature, this);

        // 核心：立即结束当前回合
        PlayerCmd.EndTurn(base.Owner, canBackOut: false);
    }

    // 升级效果：格挡 13 → 16（+3），力量不变
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}