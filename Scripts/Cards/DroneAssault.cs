using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class DroneAssault : CustomCardModel
{
    // 配置：0费 技能 代币牌 自身目标 不进图鉴
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    // 激发球体提示（和双重释放一致）
    public override OrbEvokeType OrbEvokeType => OrbEvokeType.Front;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

    // 消耗关键词
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public DroneAssault() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    // 核心：完全仿照双重释放 → 激发两次无人机
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 检查是否有无人机
        if (base.Owner.PlayerCombatState!.OrbQueue.Orbs.Count <= 0)
            return;

        // 施法动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 第一次激发（不移除无人机）
        await OrbCmd.EvokeNext(choiceContext, base.Owner, dequeue: false);
        await Cmd.CustomScaledWait(0.1f, 0.25f);
        // 第二次激发（正常触发）
        await OrbCmd.EvokeNext(choiceContext, base.Owner);
    }

    // 升级：费用-1（1费→0费，和双重释放升级效果一致）
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}