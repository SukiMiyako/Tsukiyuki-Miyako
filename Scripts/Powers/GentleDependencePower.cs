using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

// 严格对齐你提供的命名空间
namespace TsukiyukiMiyako.Scripts.Powers;

public sealed class GentleDependencePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 模组标准图标路径
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    // =====================
    // 【100%复刻监听方法】获得师生羁绊时触发
    // =====================
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // 原版判断逻辑：数值增加 + 施加者是自己 + 是师生羁绊
        if (!(amount <= 0m) && applier == base.Owner && power is SenseiPower)
        {
            Flash();
            // 核心效果：获得1点能量（对标AirDrop官方写法）
            await PlayerCmd.GainEnergy(1, base.Owner.Player!);
        }
    }
}