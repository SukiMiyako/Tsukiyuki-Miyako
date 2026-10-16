using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「这可比枪好使多了」——0 能量 + 3 弹夹(星) / 技能 / 普通 / 自身。
/// 将 2 张随机[配备]牌加入手牌，本回合免费打出。
///
/// 定位：把弹夹**换成配备**的转换件。
/// 对照同池的两张出口：`军备箱` 1 费换 2 张（不花弹夹）、`帮大忙了！老师！` 0 费 + 1 羁绊换 1 张。
/// 这张走的是第三条资源（弹夹），所以给 3 弹夹换 2 张 —— 弹夹是攒出来的资源，用它换牌是"等价交换"而不是白嫖。
///
/// 生成写法照军备箱：`CardFactory.GetForCombat` + `SetToFreeThisTurn` + `AddGeneratedCardToCombat`。
/// 升级：2 张 → 3 张（照军备箱的 `CardsVar.UpgradeValueBy(1)`），生成的牌同步升级。
///
/// 关键词：**没带 SMG** —— 参照「轻装上阵」（同样是花弹夹的非攻击牌，你说是"另外的"）。
/// 要按"花弹夹就算 SMG"的绝对口径处理，一行加上即可。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class BetterThanAGun : CustomCardModel
{
    // 0费 + 3弹夹 | 技能 | 普通 | 自身
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>星费（弹夹）：3。</summary>
    public override int CanonicalStarCost => 3;

    /// <summary>
    /// 消耗 —— 3 弹夹换 2 张免费配备，要是能反复打就变成"有弹夹就无限开箱"，
    /// 所以按你定的改成一次性。
    /// （SMG 仍然没加：参照「轻装上阵」，花弹夹的非攻击牌默认不带。要加说一声。）
    /// </summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2)
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public BetterThanAGun() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<CardModel> forCombat = CardFactory.GetForCombat(
            base.Owner,
            base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.CanonicalKeywords.Contains(MyKeywords.Equipment)),
            base.DynamicVars.Cards.IntValue,
            base.Owner.RunState.Rng.CombatCardGeneration);

        foreach (CardModel item in forCombat)
        {
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(item);
            }

            item.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, base.Owner, CardPilePosition.Random);
        }
    }

    protected override void OnUpgrade()
    {
        // 2 张 → 3 张
        base.DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
