using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;

// 模组命名空间（你所有卡牌统一格式）
namespace TsukiyukiMiyako.Scripts.Cards;

// 模组卡牌特性（必加）
[Pool(typeof(MiyakoCardPool))]
public sealed class Doubt : CustomCardModel
{
    // 不可升级
    public override int MaxUpgradeLevel => 0;

    // 你指定的词条格式
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust
    };

    // 模组固定肖像路径
    public override string PortraitPath => $"res://Tsukiyuki Miyako/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    // 回合结束手牌效果（对标 Shame）
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
        await PowerCmd.Apply<DoubtPower>(base.Owner.Creature, 1m, null, this);
    }
}