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
/// 「帮大忙了！老师！」——0 能量 / 技能 / 普通 / 自身。
/// 消耗 1 层[师生羁绊]，把 1 张随机[配备]牌加入手牌（本回合免费打出）。
///
/// 双重身份：
/// 1. **配备的出口** —— 配备牌整体要挪去做 token，池子里的入口越少越容易"抽不到生成器就死"，
///    这张 0 费牌是最好打的那一个（打不打得出来只看羁绊）。
/// 2. **寂寞路线的开关** —— `SenseiPower.AllowNegative = true`，所以羁绊到 0 之后再打这张，
///    羁绊会走到负数、挂上[寂寞]，正好接「兔子的獠牙」那条爆发线。
///
/// 定价参照：`军备箱` 1 费给 2 张（随机配备 ≈ 0.5~0.6 费/张，见 design-notes §1.2）。
/// 这张是 0 费 + 1 层羁绊（羁绊是稀缺的正向资源，且有"掉进寂寞"的机会成本）→ 1 张。
///
/// 升级：生成的配备牌也会是升级过的（数量不变），描述用 {IfUpgraded} 显示 —— 照军备箱的写法。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class ThanksSensei : CustomCardModel
{
    // 0费 | 技能 | 普通 | 自身
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SenseiPower>(1m)
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public ThanksSensei() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1) 消耗 1 层羁绊（允许走负数 —— 这就是寂寞路线的入口）
        await PowerCmd.Apply<SenseiPower>(
            choiceContext,
            base.Owner.Creature,
            -base.DynamicVars["SenseiPower"].BaseValue,
            base.Owner.Creature,
            this);

        // 2) 给 1 张随机配备（当回合免费），写法照军备箱
        IEnumerable<CardModel> forCombat = CardFactory.GetForCombat(
            base.Owner,
            base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.CanonicalKeywords.Contains(MyKeywords.Equipment)),
            1,
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
}
