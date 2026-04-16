using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Models;

namespace TsukiyukiMiyako.Scripts.Powers;

public sealed class Rabbit31SmgPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? card)
    {
        if (base.Owner != dealer)
        {
            return 0m;
        }
        if (card == null)
        {
            return 0m;
        }
        // 判定SMG关键字卡牌
        if (!card.CanonicalKeywords.Contains(MyKeywords.Smg))
        {
            return 0m;
        }
        return base.Amount;
    }
}