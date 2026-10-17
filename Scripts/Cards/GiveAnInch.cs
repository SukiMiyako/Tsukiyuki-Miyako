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
/// 「得寸进尺」——1 能量 + 1 弹夹(星) / 攻击 / 普通 / 单体 / SMG。
///
/// 双形态，一张牌同时当**寂寞的入口**和**寂寞的出口**：
///   · 平时：**消耗** 1 层羁绊，造成 15 伤 —— 用掉了保护伞，也可能把自己推进[寂寞]
///   · [寂寞]中：**获得** 1 层羁绊，造成 12 伤 —— 往回爬，羁绊回到正数就当场结束寂寞
///
/// 12 伤偏低是故意的：寂寞自带 -N 力量/-N 敏捷（<see cref="LonelinessPower"/>），
/// 这一发本来就吃不到满额伤害，所以基础值先让一档，换"把羁绊还回来"这个出口。
///
/// 结算顺序：**先结算伤害，再改羁绊**。
/// 反过来会在[寂寞]里先吃到 LonelinessPower 的 +力量回补，等于白送伤害；
/// 现在这样打出去的数值才是玩家按卡面算得出的那个数。
///
/// 注意在寂寞里 +1 羁绊的落点（读 <see cref="SenseiPower"/> 的 AfterPowerAmountChanged）：
///   羁绊 0 → 1：LonelinessPower 被清空 = **当回合直接结束寂寞**；
///   羁绊 -1 → 0：寂寞层数从 3+|−1| 重算成 3，等于少挨一回合。
///
/// 关键词：SMG（项目规则 —— 花弹夹的牌统一是 SMG），吃 RABBIT-31 与激光镭指的增幅。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class GiveAnInch : CustomCardModel
{
    // 1费 | 攻击 | 普通 | 单体
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>星费（弹夹）：1。项目规则 —— 花弹夹的牌统一是 SMG。</summary>
    public override int CanonicalStarCost => 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { MyKeywords.Smg };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(15m, ValueProp.Move),          // 平时（消耗羁绊）的伤害
        new DynamicVar("LonelyDamage", 12m),         // [寂寞]中的伤害
        new PowerVar<SenseiPower>(1m)                // 羁绊的进出层数
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
    {
        HoverTipFactory.FromPower<SenseiPower>(),
        HoverTipFactory.FromPower<LonelinessPower>(),
        HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>()
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public GiveAnInch() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // [寂寞]判定：LonelinessPower 还在身上 = 羁绊 ≤ 0
        bool lonely = Owner.Creature.GetPowerAmount<LonelinessPower>() > 0;

        // 1. 先结算伤害（顺序不能反，理由见类注释）
        decimal damage = lonely
            ? DynamicVars["LonelyDamage"].BaseValue
            : DynamicVars.Damage.BaseValue;

        await DamageCmd.Attack(damage)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 2. 再改羁绊：平时还出去，寂寞里借回来
        decimal senseiDelta = lonely
            ? DynamicVars["SenseiPower"].BaseValue
            : -DynamicVars["SenseiPower"].BaseValue;

        await PowerCmd.Apply<SenseiPower>(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            senseiDelta,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 两个形态一起加：15 → 18、12 → 14
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["LonelyDamage"].UpgradeValueBy(2m);
    }
}
