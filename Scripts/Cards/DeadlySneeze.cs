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
/// 「致命喷嚏」——0 能量 / 技能 / 普通 / 自身。
///
/// 失去 2 点生命，获得 1 层羁绊。
///
/// 名字来自水宫子记忆大厅那个"一个喷嚏让无数玩家失去 24000 青辉石"的梗 ——
/// 机制上也照着那个梗的形状写：**她打个喷嚏就把自己搭进去了，代价从血条上出**。
///
/// 定价口径：羁绊是**永久线性收益**（每层让所有格挡牌 +1 格挡、无人机 Focus +1），
/// 所以这牌本质是"2 点生命换 1 层永久属性"。0 费是关键 ——
/// 前三场能量永远不够，它属于那种"反正费用花不完就先垫一张"的牌。
///
/// 失血用 `Unblockable | Unpowered | Move`（官方 cardHpLoss 口径）：
/// 不吃格挡、不吃减伤、也不受力量影响，跟「兔子的獠牙」一致。
///
/// 关键词：不带消耗、不带 SMG（不花弹夹）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class DeadlySneeze : CustomCardModel
{
    // 0费 | 技能 | 普通 | 自身
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(2m),
        new PowerVar<SenseiPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SenseiPower>(),
        HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>()
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public DeadlySneeze() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1) 先掉血（Unblockable | Unpowered | Move = 官方 cardHpLoss，不吃格挡与减伤）
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this);

        // 2) 再攒羁绊
        await PowerCmd.Apply<SenseiPower>(
            new BlockingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["SenseiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 羁绊 1 → 2（失血不变 —— 这张牌的代价不该被升级冲掉）
        DynamicVars["SenseiPower"].UpgradeValueBy(1m);
    }
}
