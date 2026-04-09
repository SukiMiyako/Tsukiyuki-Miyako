using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Utils;


namespace TsukiyukiMiyako.Scripts.Cards;
[Pool(typeof(MiyakoCardPool))]
public sealed class AirDrop : CustomCardModel
{
    // 关键词：放逐（标准写法，无乱码）
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 动态数值：抽牌、能量、星币、锻造（标准数组写法）
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2),
        new EnergyVar(2),
        new StarsVar(2),
    };



    public AirDrop()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    // 打出效果（清理冗余base.）
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        CustomCardModel armorybox = CombatState!.CreateCard<ArmoryBox>(Owner);
        // 加入手牌
        await CardPileCmd.AddGeneratedCardToCombat(armorybox, PileType.Hand, true);
    }

    // 升级：添加与生俱来关键词
    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}