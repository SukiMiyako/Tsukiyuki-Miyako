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

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「请站在我的身后」——1 能量 / 攻击 / 普通 / 单体。
///
/// 造成 9 点伤害，获得 1 层羁绊。
///
/// 定位：Common 档"能打又能攒"的进攻件 —— 队长的本分是挡在前面，
/// 而羁绊就是她挡在前面的资本（每层让她所有格挡牌 +1）。
/// 之前 Common 层的羁绊来源只有「请你守护着我哦」，缺口太大。
///
/// 定价口径（§1.2）：1 费单体基准 6，附带条件可到 8~10 ——
/// 这里给 9，因为 1 层羁绊是**永久线性收益**（不是一次性格挡），
/// 已经吃掉了这张牌的一部分价值。升级只加伤害，不碰羁绊层数。
///
/// 关键词：不带 SMG（不花弹夹）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class StayBehindMe : CustomCardModel
{
    // 1费 | 攻击 | 普通 | 单体
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
        new PowerVar<SenseiPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SenseiPower>(),
        HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public StayBehindMe() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1) 先打伤害（顺序不能反：羁绊变化可能牵动寂寞，进而改力量）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 2) 再把老师给的依靠攒一层
        await PowerCmd.Apply<SenseiPower>(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["SenseiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 伤害 9 → 12（羁绊层数不变）
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
