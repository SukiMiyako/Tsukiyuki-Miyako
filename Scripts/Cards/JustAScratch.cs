using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「小伤而已」——3费 / 技能 / 稀有 / 自身。
///
/// 角色落点：她嘴里没有"受伤"这个词，只有"还能继续"。
/// 拿到无实体——伤害被压到每次 1 点，表面上什么都看不出来；
/// 代价是**卸掉自己所有格挡**，拒绝一切防御手段，把这一顿自己接完。
/// 「小伤而已」既是她说给别人听的那句（外冷内软、把压力全揽在自己身上），
/// 也是这张牌的全部机制：不显示、不喊疼，但是真的在掉血。
///
/// 裂缝留着：无实体不挡多段攻击与 DoT（每次命中各 1 点），
/// 所以小伤接得住重击，接不住消耗战。
///
/// （这张牌原名「我还不能倒下」，已让给「我还不能倒下」防死牌——
/// 那句台词是"不许死"，不该用在一张只减伤、还会把格挡清空的牌上。）
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class JustAScratch : CustomCardModel
{
    // 3费 | 技能 | 稀有 | 自身
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<IntangiblePower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public JustAScratch() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 1) 获得无实体
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext,
            base.Owner.Creature,
            base.DynamicVars["IntangiblePower"].BaseValue,
            base.Owner.Creature,
            this);

        // 2) 失去所有格挡（含破格挡音效与钩子，照原版 Expose 的写法）
        await CreatureCmd.LoseBlock(base.Owner.Creature, base.Owner.Creature.Block);
    }

    protected override void OnUpgrade()
    {
        // 升级：费用 3 → 2
        base.EnergyCost.UpgradeBy(-1);
    }
}
