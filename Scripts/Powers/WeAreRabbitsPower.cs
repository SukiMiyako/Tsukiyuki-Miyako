using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

// 你的标准模组命名空间
namespace TsukiyukiMiyako.Scripts;

public sealed class WeAreRabbitsPower : CustomPowerModel
{
    // 标准配置
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 你的模组图标路径
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    // =====================
    // 【完全复刻官方减费逻辑】仅能力牌费用-1
    // =====================
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        // 只对自己的卡牌生效
        if (card.Owner.Creature != base.Owner)
            return false;

        // 只对【能力牌 Power】生效
        if (card.Type != CardType.Power)
            return false;

        // 原本费用必须大于0才减费
        if (originalCost <= 0m)
            return false;

        // 费用 - 当前层数 (1层 = -1费)
        modifiedCost = originalCost - base.Amount;

        // 最低费用限制为 0，不会负数
        if (modifiedCost < 0m)
            modifiedCost = 0m;

        return true;
    }
}