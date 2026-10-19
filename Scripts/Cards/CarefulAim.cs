using System;
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
using TsukiyukiMiyako.Scripts.Resources;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「看准再打」——1 能量 + 1 弹夹(星) / 攻击 / 罕见 / 单体 / SMG。
///
/// 伤害 = **10 + 目标已损失生命值 × 50%**，**最多 40 点**。
///
/// 上限是必须的：Boss（尤其多人模式一千多血）被打残后这个公式会直接飙到几百，
/// 所以用 `MaxDamage` 兜住 —— 未升级 40、升级后 50。
///
/// 数值构成照「RABBIT1，开始清扫」（原版 BodySlam 的写法）：
///   CalculatedDamage = CalculationBase + ExtraDamage × 乘数 = 10 + 0.5 × 目标已损失生命
/// **乘数取的是被攻击目标**（lambda 的 `target`），这是它和 RABBIT1（取自己）的核心区别。
///
/// 上限的实现：乘数在 lambda 里先被夹到 `(MaxDamage - 基础值) / 每点系数`，
/// 这样 `基础 + 系数 × 乘数` 天然不超过 MaxDamage，而且升级改基础值也照样兜得住。
/// lambda 只读 `card`/`target` 两个参数与卡面变量，没有闭包（引擎要求 multiplier 必须是静态的）。
///
/// 注意口径：40 是**这张牌打出的基础伤害**上限；力量、易伤等修正仍照常在这个数字之上结算。
///
/// 关键词：SMG（项目规则 —— 花弹夹的牌统一是 SMG）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class CarefulAim : CustomCardModel
{
    // 1费 + 1弹夹 | 攻击 | 罕见 | 单体
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private const string _maxDamageKey = "MaxDamage";

    /// <summary>星费（弹夹）：1。</summary>
    // 弹夹费用：1 发（自有资源 MagazineResource，不再用储君的星）
    private const int magazineCost = 1;
    /// <summary>
    /// SMG（项目规则 —— 花弹夹的牌统一是 SMG）+ 消耗。
    /// 加消耗的原因：封顶 40 之后它还是"1 费 + 1 弹夹打 40"的补刀件，
    /// 只要能反复打就永远超模，所以改成一次性（每张牌每场战斗一次）。
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, MyKeywords.Smg };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(10m),                     // 固定 10
        new ExtraDamageVar(0.5m),                        // 每 1 点目标已损失生命 → 0.5 伤
        new DynamicVar(_maxDamageKey, 40m),              // 伤害上限
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            if (target == null)
            {
                return 0m;
            }

            decimal missingHp = target.MaxHp - target.CurrentHp;
            decimal baseValue = card.DynamicVars.CalculationBase.BaseValue;
            decimal perPoint = card.DynamicVars.ExtraDamage.BaseValue;
            decimal cap = card.DynamicVars[_maxDamageKey].BaseValue;

            if (perPoint <= 0m || missingHp <= 0m)
            {
                return 0m;
            }

            // 把乘数夹到"刚好打满上限"的位置，总伤害就永远不会超过 MaxDamage
            decimal maxMultiplier = Math.Max(0m, (cap - baseValue) / perPoint);
            return Math.Min(missingHp, maxMultiplier);
        })
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public CarefulAim() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        CustomResources<MagazineResource>.SetCanonicalCost(this, magazineCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级：上限 40 → 50（公式本身不变）
        DynamicVars[_maxDamageKey].UpgradeValueBy(10m);
    }
}
