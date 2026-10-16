using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「我还不能倒下」——2费 / 能力 / 稀有 / 自身。
///
/// 角色落点：她角色文本里的事件台词「不行……我还不能在这里倒下。」就是这张牌。
/// 这是她的**底线**，不是她的抗性：牌不还给格挡，也不回血，
/// 只把这一次死亡撤掉，把命收到 1 点——然后下一步还得她自己走。
///
/// 一次性：授予的「一步不退」只有一层，触发过一次就移除（见 NotOneStepBackPower）。
/// 也就是"每场战斗一次的保命"，比不上 LizardTail（每局一次、回 50%）的量，
/// 但它是可以靠抽牌反复来的。
///
/// 顺带说明：跟「小伤而已」（JustAScratch）的无实体叠在一起时，伤害先被压到 1 点，
/// 所以根本走不到致命判定，这张牌会一直躺在能力栏里等着那一下真正的致命。
///
/// 升级给「固有」：这牌是一次性的保命，开局握在手里比省 1 费值钱。
///
/// 实装细节见 <see cref="NotOneStepBackPower"/>（死亡层钩子 ShouldDie / AfterPreventingDeath）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class ICantFallHere : CustomCardModel
{
    // 2费 | 能力 | 稀有 | 自身
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<NotOneStepBackPower>()
    ];

    public ICantFallHere() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<NotOneStepBackPower>(
            choiceContext,
            base.Owner.Creature,
            1m,
            base.Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 升级：获得固有（开局必在手）。
        // 这牌是一次的保命，早一回合握在手里比省 1 费值钱得多——费用保持 2。
        AddKeyword(CardKeyword.Innate);
    }
}
