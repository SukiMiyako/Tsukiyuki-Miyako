using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class DualPortRecoilCompensator : CustomCardModel
{
    // 关键词：配备 + 消耗 + SMG
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        MyKeywords.Equipment,
        CardKeyword.Exhaust
    };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
   {
        new PowerVar<FreeSmgPower>(1m)
   };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 2费 | 技能牌 | 稀有 | 目标自身
    public DualPortRecoilCompensator()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // ：动画 + 施加1次免费SMG
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<FreeSmgPower>(new BlockingPlayerChoiceContext(), Owner.Creature, DynamicVars["FreeSmgPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FreeSmgPower"].UpgradeValueBy(1m);
    }
}