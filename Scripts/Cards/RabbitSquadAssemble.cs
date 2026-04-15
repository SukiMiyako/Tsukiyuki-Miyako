using System;
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
using Tsukiyuki_Miyako.MiyakoModCode.Character;
// 引入你的自定义关键词
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class RabbitSquadAssemble : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(1) // 生成1张小队支援
    };

    public RabbitSquadAssemble()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust
    };
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 完全照搬军备箱：筛选【Support】关键词的卡牌（小队支援）
        IEnumerable<CardModel> forCombat = CardFactory.GetForCombat
        (base.Owner,
         base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
         .Where(c => c.CanonicalKeywords.Contains(MyKeywords.Support)), // 👈 严格用你的Support关键字
         base.DynamicVars.Cards.IntValue,
         base.Owner.RunState.Rng.CombatCardGeneration);

        foreach (CardModel item in forCombat)
        {
            // 升级后卡牌自动升级（原版逻辑）
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(item);
            }
            // 本回合免费打出（原版逻辑）
            item.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}