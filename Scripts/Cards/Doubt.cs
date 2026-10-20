using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class Doubt : CustomCardModel
{
    // 不可升级
    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 回合结束手牌效果
    public override bool HasTurnEndInHandEffect => true;

    /// <summary>
    /// 不能被"随机生成"类效果抽到。
    ///
    /// 原版的状态牌（伤口 / 灼伤 / 眩晕……）挂在 `StatusCardPool` 里，而我们在
    /// `[Pool(typeof(MiyakoCardPool))]`——也就是说它天然混在**角色的可玩牌池**里。
    /// `CardFactory.FilterForCombat` 只排除 Basic / Ancient / Event 三档稀有度，
    /// `CardRarity.Status` 拦不住，于是没有按类型过滤的生成效果（发现 / 选择悖论 /
    /// 疯狂科学 / 泼溅……）会把状态牌当成正常牌抽进三选一。
    /// 关掉这个开关后所有走 `FilterForCombat` 的路径都会把它筛掉；
    /// 我们自己的产牌（无法拒绝的条件 / 碎在雨中的信念）走的是 `CombatState.CreateCard`，不受影响。
    /// </summary>
    public override bool CanBeGeneratedInCombat => false;

    // 1费 状态牌 无目标
    public Doubt()
        : base(1, CardType.Status, CardRarity.Status, TargetType.None)
    {
    }

    // 回合结束留在手牌触发
    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await Cmd.Wait(0.25f);
        await PowerCmd.Apply<DoubtPower>(
            new BlockingPlayerChoiceContext(),
            base.Owner.Creature,
            1m,
            null,
            this);
    }
}