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

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// RABBIT1，开始清扫：衍生攻击牌，只能由「兔子的獠牙」生成。
/// 伤害 = 50 + 自身已损失生命 × 50%（越惨越痛）。
///
/// ★ 奥义牌定位：**打出后把 1 层羁绊还回来，主动退出[寂寞]**。
/// 于是整个寂寞爆发被这张牌包成闭环：
///   獠牙（进寂寞、+10力量、+3能量）→ 清扫（大爆发）→ 羁绊 +1 → 寂寞当场结束
///   （RabbitFangPower.EndFang 把力量与敏捷惩罚一并撤销，回到正轨）
/// 这样玩家不需要另外抽到羁绊来源才能脱身 —— 爆发窗口由这张牌自己收尾。
///
/// ★ 虚无：**留不到下回合**。獠牙给的 3 点能量就是为了让它当回合能打出去，
/// 所以"打不打"是当场必须做的抉择 —— 要么这一回合激情互秒，要么放弃它。
///
/// 数值构成照原版 BodySlam 的写法：
/// CalculatedDamage = CalculationBase + ExtraDamage × 乘数 = 50 + 0.5 × 已损失生命。
/// 只用一条 CalculatedDamageVar，同时负责"结算读数"与"卡面显示"，不再另开 DamageVar。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class Rabbit1Sweep : CustomCardModel
{
    // 3费 | 攻击 | 衍生（Token）| 单体
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(50m),                 // 固定 50
        new ExtraDamageVar(0.5m),                    // 每 1 点已损失生命 → 0.5 伤害
        new PowerVar<SenseiPower>(1m),               // 打完把老师还回来：主动退出寂寞
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            card.Owner.Creature.MaxHp - card.Owner.Creature.CurrentHp)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Rabbit1Sweep() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 1) 奥义本体：按已损失生命放大的一击
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 2) 收招：把羁绊还回来 → 羁绊回到正数，寂寞当场结束
        //    （獠牙加的力量与敏捷惩罚由 RabbitFangPower.EndFang 一并撤销，回到正轨）
        //    顺序必须在伤害之后：先退出寂寞的话，獠牙那 +力量会先没掉。
        await PowerCmd.Apply<SenseiPower>(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["SenseiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    /// <summary>
    /// ★ 虚无（Ethereal）：**留不到下回合，当回合不打就烧掉**。
    ///
    /// 这条是有意堵死"囤着不打"的玩法 —— 獠牙生成它时会顺手给 3 点能量，
    /// 所以"打不打"永远是一个当场就得做的抉择：
    ///   要么这一回合用这张牌激情互秒，要么放弃它、回头用别的多段牌慢慢磨。
    ///
    /// 注意别再给它加 `HasTurnEndInHandEffect`：`CombatManager.DoTurnEnd` 里
    /// 手牌被动与虚无是 `else if` 互斥的（CombatManager.cs:1224-1232），
    /// 一旦实现了手牌被动，虚无就不会触发，等于白写。
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(10m);  // 50 → 60
        DynamicVars.ExtraDamage.UpgradeValueBy(0.1m);     // 50% → 60%
    }
}
