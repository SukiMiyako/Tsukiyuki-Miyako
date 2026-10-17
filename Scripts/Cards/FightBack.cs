using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「该反击了」——2费 / 技能 / 普通 / 自身。
///
/// 获得 16 点格挡；**若到你的下个回合开始为止都没有失去生命值**，
/// 则对所有敌人造成 20 点伤害。
///
/// 判定时机：卡牌打出 → 挂上 <see cref="FightBackPower"/> → 敌方回合打完 →
/// 你的下个回合开始时结算（没掉血就打，掉过血就安静消失）。
/// 也就是说它考的是"这一轮你的盾有没有被打穿"，正好是"闪避坦"的题。
///
/// 判定用生命值变化而不是"受到伤害"：**被格挡住的攻击不算**，这是这张牌的核心。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class FightBack : CustomCardModel
{
    // 2费 | 技能 | 普通 | 自身
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(16m, ValueProp.Move),
        new DamageVar(20m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FightBackPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public FightBack() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1) 举盾
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        // 2) 挂状态：反击伤害直接作为 power 的 Amount（提示框才认 {Amount}）
        var power = await PowerCmd.Apply<FightBackPower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars.Damage.BaseValue,
            base.Owner.Creature,
            this);

        power?.SetSource(this);
        power?.SetSource(this);
    }

    protected override void OnUpgrade()
    {
        // 反击伤害 20 → 25（格挡不变）
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
