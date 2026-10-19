using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;
using TsukiyukiMiyako.Scripts.Resources;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// 「下回合装填弹夹」——原版 <c>StarNextTurnPower</c> 的自有资源版本。
///
/// 为什么需要它：星改成本地资源后，任何"下回合获得星"的牌都不能再用原版的
/// `StarNextTurnPower`（它调的是 `PlayerCmd.GainStars`，那是储君的资源）。
/// 这个 power 在**能量重置时**（也就是新回合开始、拿到能量之后）把 Amount 发成弹夹，然后自移除。
///
/// `AfterEnergyReset` 的时机与原版一致，所以手感（下回合开头到账）不会变。
/// </summary>
public sealed class MagazineNextTurnPower : MiyakoModPower
{
    public override PowerType Type => PowerType.Buff;

    // 层数 = 下回合装填几发
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }

        CustomResources<MagazineResource>.Get(player.PlayerCombatState!).ModifyAmount((int)base.Amount);
        await PowerCmd.Remove(this);
    }
}
