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
/// 「兔子的獠牙」打出去的那口气，现在是**一个状态**（有图标）。
///
/// 它不是单纯的增益，而是"只在寂寞里成立"的爆发：
///   力量 +Amount、敏捷 −5（见 <see cref="DexterityPenalty"/>），
///   而「寂寞」一结束（羁绊回到正数、LonelinessPower 被清空），
///   加伤与减防**一起撤销**。
///   只有 [脆弱] 留在身上 —— 那是代价，也是她用力的证据。
///
/// 图标上的数字 = 这一口拿到了多少力量（普通 10，升级 14）。
///
/// 触发点说明：
///   寂寞结束时，SenseiPower 会走 `PowerCmd.Apply&lt;LonelinessPower&gt;(-N)` 把它清掉；
///   在 `PowerCmd.ModifyAmount` 里是先 `SetAmount` 再派发 `Hook.AfterPowerAmountChanged`
///   （PowerCmd.cs:236-245），所以这个钩子里读到的 Loneliness 已经是 0 —— 就是"结束"的瞬间。
///
/// 同一时刻 LonelinessPower 自己也会把 −N 力量/−N 敏捷还回来（它的堆叠 debuff）；
/// 两边都是加算，先后顺序不影响结果。
/// </summary>
public sealed class RabbitFangPower : MiyakoModPower
{
    /// <summary>每次发动獠牙的敏捷代价：固定 5，不随升级变化。</summary>
    public const decimal DexterityPenalty = 5m;

    public override PowerType Type => PowerType.Buff;

    // 层数 = 这一口拿到的力量，图标上直接看得见
    public override PowerStackType StackType => PowerStackType.Counter;

    // 图标按项目现有命名约定找 tsukiyukimiyako-rabbit_fang_power.png，缺图回落到通用图标
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
        // 只关心"寂寞被削减"这一件事；并且要确认它真的见底了
        if (power is not LonelinessPower || amount >= 0m)
        {
            return;
        }
        if (base.Owner.GetPowerAmount<LonelinessPower>() > 0)
        {
            return;
        }

        await EndFang(choiceContext);
    }

    /// <summary>寂寞结束：把加伤与减防一起收回去，只留脆弱。</summary>
    private async Task EndFang(PlayerChoiceContext choiceContext)
    {
        Flash();

        // 撤销力量（Amount = 当初加了多少）
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            base.Owner,
            -base.Amount,
            null,
            null,
            silent: true);

        // 撤销敏捷削减
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            base.Owner,
            DexterityPenalty,
            null,
            null,
            silent: true);

        // 状态图标消失；FrailPower 不碰
        await PowerCmd.Remove(this);
    }
}
