using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「扫射」——1 能量 + 2 弹夹(星) / 攻击 / 普通 / 全体 / 3 段 × 3 伤。
///
/// 定位：补 §1.9 那个窟窿 —— 配备牌整体 token 化之后，**奖励池里 1 费档的群攻会空掉**
/// （原本只有「高爆手雷」一张），所以补一张进池的 1 费群攻。
///
/// 定价口径（§1.2）：1 费 ≈ 2 弹夹，所以这张牌实际成本 ≈ 2 费；
/// 2 费的单段群攻标准是 12~16 —— 这里给 9（每段 3×3），
/// 一是因为多段会被蜂群术士的 Personal Hive 按命中次数惩罚（同比应略降），
/// 二是它带 SMG 标签，能吃 `RABBIT-31`(+2/段) 与 `激光镭指`(额外打一次) 的增幅。
///
/// 写法对齐同族：星费 = `CanonicalStarCost`（短点射 1 / 冲锋爆发 1 / 抵近射击 2），
/// 升级 = 每段 +1 伤（9 → 12）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class Strafe : CustomCardModel
{
    // 1费 | 攻击 | 普通 | 全体
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>扫射 = SMG 体系（吃 RABBIT-31 的 +2/段、激光镭指的额外一击）。</summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { MyKeywords.Smg };

    /// <summary>星费（弹夹）：2。</summary>
    public override int CanonicalStarCost => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new SmgRepeatVar(3)
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Strafe() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCardCompatibility(this, cardPlay)
            .TargetingAllOpponents(base.CombatState!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 每段 +1（3×3 → 4×3，全体 9 → 12）
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
