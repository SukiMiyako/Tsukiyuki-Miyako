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

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// 「离我远点！」的状态图标 —— 寂寞期那点缓冲的"记账本"。
///
/// 层数 = 这张牌给出去的缓冲层数。寂寞一结束（LonelinessPower 被清空），
/// **没消耗掉的缓冲一起收回**，图标消失 —— RABBIT 关键词的铁律：
/// 寂寞里的增益不跟着带出寂寞，不给白嫖。
///
/// 和 <see cref="RabbitFangPower"/> / <see cref="ImFinePower"/> 是同一个套路：
/// 数值由卡牌当场给，power 只负责"记账 + 到点收回"。
///
/// 收的时候要夹一下：缓冲可能已经被敌人打掉了（每挨一次穿透伤害减 1 层），
/// 直接 `-Amount` 会试图扣成负数，所以只扣"当前还剩的那些"。
///
/// 触发点：寂寞结束时 SenseiPower 走 `PowerCmd.Apply&lt;LonelinessPower&gt;(-N)` 清零；
/// `PowerCmd.ModifyAmount` 是先 SetAmount 再派发 AfterPowerAmountChanged（PowerCmd.cs:236-245），
/// 所以这里读到的 Loneliness 已经是 0 —— 正是"结束的瞬间"。
/// </summary>
public sealed class StayAwayPower : MiyakoModPower
{
    public override PowerType Type => PowerType.Buff;

    // 层数 = 给了多少缓冲
    public override PowerStackType StackType => PowerStackType.Counter;

    // 图标：按项目命名约定找 tsukiyukimiyako-stay_away_power.png，缺图回落通用图标
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

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // 只关心"寂寞被削减"，并且要确认它真的见底了
        if (power is not LonelinessPower || amount >= 0m)
        {
            return;
        }
        if (base.Owner.GetPowerAmount<LonelinessPower>() > 0)
        {
            return;
        }

        // 寂寞结束：收回还没被消耗掉的缓冲
        int remaining = base.Owner.GetPowerAmount<BufferPower>();
        int toRemove = (int)Math.Min(base.Amount, (decimal)remaining);
        if (toRemove > 0)
        {
            await PowerCmd.Apply<BufferPower>(choiceContext, base.Owner, -toRemove, null, null, silent: true);
        }

        await PowerCmd.Remove(this);
    }
}
