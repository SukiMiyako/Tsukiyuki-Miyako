using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class ShiningRabbit : CustomCardModel
{
    // 动态变量：施加Power
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new PowerVar<ShiningRabbitPower>(1m)
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 1费 蓝色能力卡
    public ShiningRabbit()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 打出动画+施加Power
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        // 仅修复：
        await PowerCmd.Apply<ShiningRabbitPower>(new BlockingPlayerChoiceContext(), Owner.Creature, DynamicVars["ShiningRabbitPower"].BaseValue, Owner.Creature, this);
    }

    // 升级参考 Armaments 规范
    // 升级后：前2张牌免费（Power层数+1）
    protected override void OnUpgrade()
    {
        DynamicVars["ShiningRabbitPower"].UpgradeValueBy(1m);
    }
}