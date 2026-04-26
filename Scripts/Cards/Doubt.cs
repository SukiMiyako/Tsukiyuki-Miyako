using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Cards;

[Pool(typeof(MiyakoCardPool))]
public sealed class Doubt : CustomCardModel
{
    // 不可升级
    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust
    };

    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 回合结束手牌效果
    public override bool HasTurnEndInHandEffect => true;

    // 1费 状态牌 无目标
    public Doubt()
        : base(1, CardType.Status, CardRarity.Status, TargetType.None)
    {
    }

    // 回合结束留在手牌触发
    public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await Cmd.Wait(0.25f);
        // 🔥 唯一修复：补全官方标准 PlayerChoiceContext 参数
        await PowerCmd.Apply<DoubtPower>(
            new BlockingPlayerChoiceContext(),
            base.Owner.Creature,
            1m,
            null,
            this);
    }
}