using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TsukiyukiMiyako.Scripts.Resources;

namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// 「弹药管够」——每花 <see cref="Threshold"/> 发弹夹，返 1 发。
///
/// 弹夹从原版的[gold]星[/gold]改成本地资源 <see cref="MagazineResource"/> 之后，
/// 原来监听 `AfterStarsSpent` 的写法失效了，改为实现 BaseLib 的
/// <see cref="IAfterSpendResource{T}"/>（`BaseLibHooks.AfterSpendCustomResource` 会派发给它）。
///
/// 记账口径不变：累计花费 / 阈值 - 已触发次数，一次花很多发也能连触发。
/// `DisplayAmount` 显示"距离下一次返弹还差几发"，图标上的数字会跟着变。
/// </summary>
public sealed class AmmoSurplusPower : CustomPowerModel, IAfterSpendResource<MagazineResource>
{
    /// <summary>每花多少发返一次。</summary>
    private const int Threshold = 5;

    private class Data
    {
        /// <summary>累计花掉的弹夹。</summary>
        public int totalSpent;

        /// <summary>已经触发过的次数。</summary>
        public int triggerCount;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>还差几发触发下一次（图标上显示这个数）。</summary>
    public override int DisplayAmount => Threshold - GetInternalData<Data>().totalSpent % Threshold;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";

    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    /// <summary>每次返几发。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Magazine", 1m) };

    protected override object InitInternalData()
    {
        return new Data();
    }

    public Task AfterSpendResource(
        ICombatState combatState, MagazineResource resource, AbstractModel? spender, int amount)
    {
        if (amount <= 0 || base.Owner.Player == null)
        {
            return Task.CompletedTask;
        }

        Data data = GetInternalData<Data>();
        data.totalSpent += amount;

        int triggers = data.totalSpent / Threshold - data.triggerCount;
        if (triggers <= 0)
        {
            InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        Flash();
        CustomResources<MagazineResource>.Get(base.Owner.Player.PlayerCombatState!)
            .ModifyAmount(DynamicVars["Magazine"].IntValue * triggers);
        data.triggerCount += triggers;

        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}
