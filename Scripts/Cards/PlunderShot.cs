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
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;
using TsukiyukiMiyako.Scripts.Resources;

namespace TsukiyukiMiyako.Scripts.Cards;

/// <summary>
/// 「掠夺射击」——1 能量 + 1 弹夹(星) / 攻击 / 罕见 / 单体。
/// 造成 12 点伤害；**若击杀了敌人**，把 1 张随机[配备]牌加入手牌（本回合免费打出）。
///
/// 定位：单段（不吃蜂群术士的 Personal Hive 惩罚）+ 击杀才能兑现的额外收益，
/// 所以它按"条件牌"定价（§1.2：前置条件型可以给到 8~10/费）。
///
/// 击杀判定照「高空突袭」（Sunder 写法）：
///   `attackResult.Results.SelectMany(r => r).Any(r => r.WasTargetKilled)`
/// 配备的生成按军备箱那套（`GetForCombat` + `SetToFreeThisTurn` + 入手）。
///
/// 升级：12 → 15 伤。
///
/// 关键词：SMG（项目规则 —— 花弹夹的牌统一是 SMG），
/// 所以吃 `RABBIT-31` 的 +2 伤和 `激光镭指` 的额外一击（这张是单段，+2 只吃一次）。
/// </summary>
[Pool(typeof(MiyakoCardPool))]
public sealed class PlunderShot : CustomCardModel
{
    // 1费 | 攻击 | 罕见 | 单体
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>星费（弹夹）：1。项目规则 —— 花弹夹的牌统一是 SMG。</summary>
    // 弹夹费用：1 发（自有资源 MagazineResource，不再用储君的星）
    private const int magazineCost = 1;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { MyKeywords.Smg };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move)
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public PlunderShot() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        CustomResources<MagazineResource>.SetCanonicalCost(this, magazineCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attackResult = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 没杀掉就什么都没发生 —— 这牌的价值全在"能打死"上
        if (!attackResult.Results.SelectMany(r => r).Any(r => r.WasTargetKilled))
        {
            return;
        }

        // 击杀 → 摸 1 张随机配备（当回合免费）
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

    protected override void OnUpgrade()
    {
        // 12 → 15
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
