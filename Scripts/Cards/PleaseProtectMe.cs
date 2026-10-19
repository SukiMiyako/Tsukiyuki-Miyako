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
using TsukiyukiMiyako.Scripts;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「请你守护着我哦」——1 能量 / 技能 / 普通 / 自身。
///
/// 获得 12 点格挡；**若到你的下个回合开始为止没有失去生命值**，获得 2 层羁绊。
///
/// 定位：Common 档的羁绊来源 —— 之前整个 Common 层**一张产出羁绊的牌都没有**
/// （只有「帮大忙了！老师！」「得寸进尺」「像你这样的大人……」这些消耗方），
/// 而前三场抽到的几乎都是 Common，导致羁绊攒不起来、
/// 「那片星空，名为奇迹」「温柔依赖症」这些放大器前期空转。
///
/// 触发逻辑交给 <see cref="PleaseProtectMePower"/>（照「该反击了」的同构写法）：
/// 记一笔"这轮有没有掉血"，下回合开始结算。
///
/// 判定的是**生命值变化**而不是"受到伤害"：被格挡吃住的攻击不算掉血，
/// 所以"守住了"这件事是可以靠格挡挣到的，而格挡又吃羁绊加成 —— 这张牌自己就在滚这个循环。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class PleaseProtectMe : CustomCardModel
{
    // 1费 | 技能 | 普通 | 自身
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12m, ValueProp.Move),
        new PowerVar<SenseiPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PleaseProtectMePower>(),
        HoverTipFactory.FromPower<SenseiPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public PleaseProtectMe() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1) 先举盾
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        // 2) 挂状态：羁绊层数直接作为 power 的 Amount（提示框才认 {Amount}）
        await PowerCmd.Apply<PleaseProtectMePower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["SenseiPower"].BaseValue,
            base.Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 格挡 12 → 16（羁绊层数不变）
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
