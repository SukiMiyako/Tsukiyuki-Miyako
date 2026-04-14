using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

// 引入你制作的【劳累】状态牌
using TsukiyukiMiyako.Scripts.Cards;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class PartTime : CustomCardModel
{
    // 基础配置：0费 技能牌 蓝卡 目标自身
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 能量变量：基础3点
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(3)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public PartTime() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 核心效果：获得能量 + 将劳累加入手牌（参考内核加速源码）
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 获得能量
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

        // 创建【劳累】状态牌 并加入手牌（复刻内核加速逻辑）
        CardModel fatigueCard = CombatState!.CreateCard<Fatigue>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(fatigueCard, PileType.Draw, addedByPlayer: true);
    }

    // 升级：3能量 → 4能量
    protected override void OnUpgrade()
    {
        base.DynamicVars.Energy.UpgradeValueBy(1m);
    }
}