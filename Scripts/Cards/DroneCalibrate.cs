using BaseLib.Abstracts;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Orbs;
// 引入你的闪光无人机球体
using TsukiyukiMiyako.Scripts;
using System.Collections.Generic;
using System.Linq;

namespace TsukiyukiMiyako.Scripts;

[Pool(typeof(MiyakoCardPool))]
public sealed class DroneCalibrate : CustomCardModel
{
    // 基础配置：1费 技能牌 普通稀有 单体敌人目标
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    // 球体提示：显示闪光无人机（特斯拉线圈）
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromOrb<FlashScoutDrone>()];

    // 卡牌图片路径
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public DroneCalibrate() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    // 核心：特斯拉线圈 → 触发无人机被动3次
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 空目标判定（特斯拉线圈）
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 施法动画（沿用无人机突袭的动画）
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 获取所有（特斯拉线圈获取闪电球）
        List<FlashScoutDrone> drones = Owner.PlayerCombatState!.OrbQueue.Orbs
            .OfType<FlashScoutDrone>()
            .ToList();

        // 核心逻辑：对目标触发无人机被动
        for (int i = 0; i < 3; i++)
        {
            foreach (FlashScoutDrone drone in drones)
            {
                // 特斯拉线圈：触发球体被动 Passive
                await OrbCmd.Passive(choiceContext, drone, cardPlay.Target);
            }
        }
    }

    // 升级效果（可选，推荐：费用-1 → 0费）
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}