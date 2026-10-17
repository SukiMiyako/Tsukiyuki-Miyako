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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「像你这样的大人……」——1 能量 / 攻击 / 普通 / 单体。
///
/// 失去 1 层羁绊，造成 5 点伤害，获得 5 层[gold]荆棘[/gold]。
///
/// 名字是她的名台词「像你这样的大人最讨厌了」的半句 —— 后来成了著名回旋镖。
/// 机制上取的是"嘴上顶回去、身上长出刺"：一边把羁绊推开一点，
/// 一边给自己挂一手反伤，越是靠近她的人越容易扎到。
///
/// 荆棘（<see cref="ThornsPower"/>）的伤害是 `Unpowered` 的固定值，
/// 不吃力量/易伤，也**不随段数重复**——每次受到攻击性伤害触发一次，
/// 所以 5 层 = 每个攻击者每回合白挨 5 点。
///
/// 结算顺序：**先伤害，再扣羁绊**。反过来会先掉进[寂寞]吃 LonelinessPower 的
/// −力量，让卡面上写的 5 点打不满；现在玩家按卡面算得出。
///
/// 关键词：不带消耗（不是寂寞专属牌），不带 SMG（不花弹夹）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class AdultsLikeYou : CustomCardModel
{
    // 1费 | 攻击 | 普通 | 单体
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new PowerVar<ThornsPower>(5m),
        new PowerVar<SenseiPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ThornsPower>(),
        HoverTipFactory.FromPower<SenseiPower>(),
        HoverTipFactory.FromPower<LonelinessPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public AdultsLikeYou() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1) 先打伤害（顺序不能反，理由见类注释）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 2) 把羁绊推开一层（可能把自己推进寂寞）
        await PowerCmd.Apply<SenseiPower>(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            -DynamicVars["SenseiPower"].BaseValue,
            Owner.Creature,
            this);

        // 3) 长出刺
        await PowerCmd.Apply<ThornsPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ThornsPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 荆棘 5 → 7（伤害是 1 费基准值，不动）
        DynamicVars["ThornsPower"].UpgradeValueBy(2m);
    }
}
