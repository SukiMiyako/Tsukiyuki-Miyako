using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class PleaseStayLonger : CustomCardModel
{
    private const string _skillPlaysKey = "SkillPlays";

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>(new DynamicVar[] { new DynamicVar("SkillPlays", 1m) });
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust
    };
    public PleaseStayLonger()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PleaseStayLongerPower>(Owner.Creature, DynamicVars["SkillPlays"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SkillPlays"].UpgradeValueBy(1m);
    }
}