using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TsukiyukiMiyako.Scripts;

public sealed class RestPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 完全照搬暗影步的方法名 + 参数（官方原生支持，无报错）
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        // 仅自己的回合触发
        if (side == base.Owner.Side)
        {
            // 施加力量（层数=力量值，2层=2力量）
            await PowerCmd.Apply<StrengthPower>(base.Owner, base.Amount, base.Owner, null);
            // 触发后立即移除自身（和暗影步逻辑完全一致）
            await PowerCmd.Remove(this);
        }
    }
}