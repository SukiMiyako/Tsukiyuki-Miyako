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
/// 「我……我没事」的状态图标 —— 只在「寂寞」里成立的那口气。
///
/// 层数 = 这一口拿到了多少力量/敏捷。寂寞一结束（LonelinessPower 被清空），
/// 力量与敏捷**一起收回**，图标消失。
///
/// 和「兔子的獠牙」(<see cref="RabbitFangPower"/>) 是同一个套路：
/// 数值由卡牌当场给，power 只负责"记账 + 到点收回"，
/// 因为 power 的 Amount 只有一个格子，而这张牌的两个数值是相等的（都等于层数），所以刚好够用。
///
/// 触发点：寂寞结束时 SenseiPower 会走 `PowerCmd.Apply&lt;LonelinessPower&gt;(-N)` 把它清零；
/// `PowerCmd.ModifyAmount` 是先 SetAmount 再派发 AfterPowerAmountChanged（PowerCmd.cs:236-245），
/// 所以这个钩子里读到的 Loneliness 已经是 0 —— 正是"结束的瞬间"。
/// </summary>
public sealed class ImFinePower : MiyakoModPower
{
    public override PowerType Type => PowerType.Buff;

    // 层数 = 加成数值
    public override PowerStackType StackType => PowerStackType.Counter;

    // 图标：按项目命名约定找 tsukiyukimiyako-im_fine_power.png，缺图回落通用图标
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

        // 寂寞结束：力量与敏捷一起收回（当时给了多少就还多少）
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -base.Amount, null, null, silent: true);
        await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, -base.Amount, null, null, silent: true);

        await PowerCmd.Remove(this);
    }
}
