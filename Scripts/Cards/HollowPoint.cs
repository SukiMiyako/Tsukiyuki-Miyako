using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts;
[Pool(typeof(MiyakoCardPool))]
// 空尖弹 | 0费 攻击牌 消耗 对易伤敌人伤害翻倍
public sealed class HollowPoint : CustomCardModel
{
    // 易伤目标高亮（沿用拆卸的逻辑）
    protected override bool ShouldGlowGoldInternal => CombatState?.HittableEnemies.Any(e => e.HasPower<VulnerablePower>()) ?? false;

    // 基础伤害：6点 | 升级+2 → 8点
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new DamageVar(6m, ValueProp.Move);
        }
    }

    // 卡牌关键词：消耗 + 易伤提示
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust,
        MyKeywords.Equipment
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<VulnerablePower>();
        }
    }

    // 构造：0费 / 攻击牌 / 稀有度Uncommon / 任意敌人
    public HollowPoint()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 核心逻辑：沿用拆卸判定 → 目标有易伤 → 伤害×2
        int damage = DynamicVars.Damage.IntValue;
        if (cardPlay.Target.HasPower<VulnerablePower>())
        {
            damage *= 2;
        }
        
        // 攻击逻辑（原生攻击动画+特效）
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // 升级：伤害+2
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}