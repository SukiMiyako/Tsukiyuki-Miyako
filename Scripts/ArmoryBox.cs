using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;




namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class ArmoryBox : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {// 基础伤害：25点
        new CardsVar(2)                      // 生成卡牌数量：3张
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public ArmoryBox()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        IEnumerable<CardModel> forCombat = CardFactory.GetForCombat
        (base.Owner, base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint).Where(delegate (CardModel c)
        {
            CardEnergyCost energyCost = c.EnergyCost;
            return energyCost != null && energyCost.Canonical == 0 && !energyCost.CostsX;
        }), base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatCardGeneration);
        foreach (CardModel item in forCombat)
        {
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(item);
            }
            await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1m);
    }
}