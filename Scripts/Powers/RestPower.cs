using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TsukiyukiMiyako.Scripts;

public sealed class RestPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    // 🔥 完全官方签名：CombatState → ICombatState（对标所有官方能力）
    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == base.Owner.Side)
        {
            Flash();
            // 🔥 官方标准临时力量施加（严格对齐TemporaryStrengthPower逻辑）
            await PowerCmd.Apply<TemporaryStrengthPower>(
                new BlockingPlayerChoiceContext(), // 官方要求的第一个参数：PlayerChoiceContext
                base.Owner,                         // 目标：自己
                base.Amount,                        // 数值：等于本能力层数
                base.Owner,                         // 施加者：自己
                null);
            // 官方标准获得能量
            await PlayerCmd.GainEnergy(1, base.Owner.Player!);
            // 官方标准移除自身
            await PowerCmd.Remove(this);
        }
    }

    // 🔥 额外补全官方通用的回合结束方法（和TemporaryStrengthPower完全一致）
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == base.Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}