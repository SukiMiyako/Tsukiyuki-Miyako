using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class WavesLonging : CustomCardModel
{
    // 动态变量：消耗1层师生羁绊、抽2张牌
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new PowerVar<SenseiPower>(1m),
        new CardsVar(2)
    };

    // 悬浮提示：显示师生羁绊
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
    {
        HoverTipFactory.FromPower<SenseiPower>()
    };

    // 标准图标路径
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 0费 | 技能牌 | 稀有 | 目标自身
    public WavesLonging()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 标准施法动画
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 核心：消耗1层师生羁绊（加负号）
        await PowerCmd.Apply<SenseiPower>(Owner.Creature, -DynamicVars["SenseiPower"].BaseValue, Owner.Creature, this);
        // 抽2张牌
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    // 升级：抽牌数+1（抽3张）
    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}