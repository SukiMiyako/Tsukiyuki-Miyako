using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TsukiyukiMiyako.Scripts;

public sealed class AmmoSurplusPower : CustomPowerModel
{
    // 完全复刻Orbit 内部数据类
    private class Data
    {
        public int starsSpent;
        public int triggerCount;
    }

    // 阈值：每花费5辉星触发
    private const int _starsIncrement = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // 显示：还需要花费多少辉星触发
    public override int DisplayAmount => 5 - GetInternalData<Data>().starsSpent % 5;

    // 模组标准图标路径
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    public override bool IsInstanced => true;
    // 动态变量：获得1辉星
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new StarsVar(1) };

    // 初始化数据
    protected override object InitInternalData()
    {
        return new Data();
    }

    // =====================
    // 【严格复刻 ChildOfTheStarsPower 监听写法】
    // 无Card参数，参数完全一致
    // =====================
    public override async Task AfterStarsSpent(int amount, Player spender)
    {
        // 官方原版判断条件
        if (amount > 0 && spender == base.Owner.Player)
        {
            Data data = GetInternalData<Data>();
            // 累计花费的辉星
            data.starsSpent += amount;
            // 计算触发次数
            int triggers = data.starsSpent / _starsIncrement - data.triggerCount;

            if (triggers > 0)
            {
                Flash();
                // 【严格对标AirDrop】获得辉星
                await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue * triggers, base.Owner.Player);
                data.triggerCount += triggers;
            }
            InvokeDisplayAmountChanged();
        }
    }
}