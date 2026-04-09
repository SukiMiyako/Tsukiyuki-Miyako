using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Tsukiyuki_Miyako.MiyakoModCode.Cards;

[Pool(typeof(MiyakoCardPool))]
public abstract class MiyakoModCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{// 大图（卡牌详情界面）
    public override string CustomPortraitPath =>
        $"res://Tsukiyuki Miyako/images/card_portraits/big/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    // 小图（战斗/卡组界面）
    public override string PortraitPath =>
        $"res://Tsukiyuki Miyako/images/card_portraits/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    // Beta 图
    public override string BetaPortraitPath =>
        $"res://Tsukiyuki Miyako/images/card_portraits/beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
}