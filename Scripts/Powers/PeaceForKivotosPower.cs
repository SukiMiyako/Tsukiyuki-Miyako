using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// 「为了基沃托斯的和平」——**把羁绊持续换成力量**。
///
/// 每 <c>Amount</c> 层羁绊 → +1 点[gold]力量[/gold]，羁绊一变就重新结算一次
/// （涨了补、掉了收，见 <see cref="SyncStrength"/>）。
///
/// 为什么改成这样：原版这张牌只是一句"获得 X 点力量"，而她池子里的力量来源已经很多
/// （兔子的獠牙 / 我……我没事 / 遗物），纯加力量既重复又没有辨识度。
/// 改成"羁绊 → 力量"之后：
///   · 它成了**羁绊的第二个兑现口**（第一个是格挡），把"攒羁绊"这条主线接进输出
///   · 与「与你一起」（消耗全部羁绊换一发大的）形成反向选择：
///     要么留着羁绊持续吃力量，要么一次性打出去
///   · 寂寞时羁绊 ≤ 0 → 力量自动归 0，跟「寂寞里什么都没有」的设定一致
///
/// 记账方式照 <see cref="RabbitFangPower"/>：power 只负责"记已经给了多少"，
/// 差额用 `PowerCmd.Apply&lt;StrengthPower&gt;` 补/收，所以显示出来的是**真力量**，
/// 会正常参与伤害结算，也会和獠牙的 +力量线性叠加。
///
/// 图标上的数字 = 现在已经提供了多少力量（`DisplayAmount`）。
/// 悬停提示只能认 `{Amount}`（见 PowerModel.HoverTips 的限制），所以说明里写的是"每 {Amount} 层"。
/// </summary>
public sealed class PeaceForKivotosPower : MiyakoModPower
{
    public override PowerType Type => PowerType.Buff;

    // 层数 = 每几层羁绊换 1 点力量
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>图标上直接显示"现在给了几点力量"。</summary>
    public override int DisplayAmount => _granted;

    /// <summary>已经发放出去的力量，用来算差额。</summary>
    private int _granted;

    public override string CustomPackedIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/powers/power.png";
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/powers/big/power.png";
        }
    }

    /// <summary>卡牌打出后立刻结算一次（羁绊已经存在时也要给到）。</summary>
    public async Task SyncStrength(PlayerChoiceContext choiceContext)
    {
        int ratio = Math.Max(1, (int)base.Amount);
        int bond = Math.Max(0, base.Owner.GetPowerAmount<SenseiPower>());
        int target = bond / ratio;
        int delta = target - _granted;

        if (delta == 0)
        {
            return;
        }

        _granted = target;
        Flash();
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, base.Owner, delta, null, null, silent: true);
        InvokeDisplayAmountChanged();
    }

    /// <summary>羁绊一变动就同步（增删都会走到这里）。</summary>
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power is not SenseiPower)
        {
            return Task.CompletedTask;
        }

        return SyncStrength(choiceContext);
    }
}
