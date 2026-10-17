using System;
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
/// 「与你一起」——2 能量 / 攻击 / 罕见 / 单体。
///
/// **消耗你所有的羁绊**，造成 10 + 羁绊层数 × 6 的伤害。
///
/// 定位：羁绊的"兑现牌"（§1.9 说的那种把攒下来的资源一次性打出去）。
/// 代价是全部保护伞——羁绊每层既是格挡牌 +1 格挡，也是无人机 Focus，
/// 打完之后如果正好归零，还会当场掉进[寂寞]（`SenseiPower` 归零即挂 `LonelinessPower`），
/// 所以这张牌实际上是"把安全余量换成一颗子弹，然后自己站到没有余量的地方"。
///
/// 数值构成照原版 BodySlam / 同族的 RABBIT1 写法：
/// CalculatedDamage = CalculationBase + ExtraDamage × 乘数 = 10 + 6 × 羁绊层数。
/// 乘数取卡主自己的羁绊层数（lambda 里读 `card.Owner`，无闭包）。
///
/// 结算顺序：**先按当前羁绊算伤害并结算，再把羁绊清空**。
/// 顺序反了的话伤害会变成 10 点（羁绊先没了），卡面公式就骗人了。
///
/// 关键词：不带消耗（不是寂寞专属牌），不带 SMG（不花弹夹）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class TogetherWithYou : CustomCardModel
{
    // 2费 | 攻击 | 罕见 | 单体
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(10m),                 // 固定 10
        new ExtraDamageVar(6m),                      // 每 1 层羁绊 → 6 伤害
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
        {
            // 羁绊允许为负（SenseiPower.AllowNegative），但伤害不能再往下扣 —— 用 0 兜底
            if (card.Owner == null)
            {
                return 0m;
            }
            return Math.Max(0m, (decimal)card.Owner.Creature.GetPowerAmount<SenseiPower>());
        })
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SenseiPower>(),
        HoverTipFactory.FromPower<LonelinessPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public TogetherWithYou() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 1) 先按当前羁绊结算伤害（顺序不能反，理由见类注释）
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 2) 再把羁绊清空。归零会触发寂寞（3 层），这是这张牌的代价
        int sensei = Owner.Creature.GetPowerAmount<SenseiPower>();
        if (sensei > 0)
        {
            await PowerCmd.Apply<SenseiPower>(
                new BlockingPlayerChoiceContext(),
                Owner.Creature,
                -sensei,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        // 系数 6 → 8（例：5 层羁绊 40 → 50 伤）
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}
