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
public sealed class ArmoryBox : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2) // 基础生成2张，升级+1
    };

    public ArmoryBox()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 核心修改：筛选 带有【配备】关键词 的卡牌（替换原0费筛选）
        IEnumerable<CardModel> forCombat = CardFactory.GetForCombat
        (base.Owner,
         base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
         .Where(c => c.CanonicalKeywords.Contains(MyKeywords.Equipment)), // 👈 关键修改
         base.DynamicVars.Cards.IntValue,
         base.Owner.RunState.Rng.CombatCardGeneration);

        foreach (CardModel item in forCombat)
        {
            // 原版逻辑：升级后卡牌自动升级
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(item);
            }
            // 新增：本回合免费打出（声东击西官方逻辑）
            item.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1m);
    }
}