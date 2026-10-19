using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// 「请你守护着我哦」的状态：撑住这一轮，撑住了就有回报。
///
/// 逻辑与「蓄势反击」(<see cref="FightBackPower"/>) 同构，只是把"还手"换成"获得羁绊"：
///   1. 卡牌打出时挂上本 power（Amount = 下回合给几层羁绊）；
///   2. 期间只要**失去过生命值**（`AfterCurrentHpChanged` 收到负 delta）就记一笔；
///   3. 到你的**下个回合开始**结算：没掉过血 → 获得 Amount 层羁绊；掉过血 → 安静消失。
///
/// 判定的是**生命值变化**而不是"受到伤害"：被格挡完全吃住的攻击不会算掉血，
/// 这正是这张牌想要的口径 —— 守住就算数。
///
/// 层数存在 `Amount` 里而不是自定义 DynamicVar：能力提示框只会自动注入 `{Amount}`。
/// 重复打出会累加（两张 = 下回合 4 层羁绊）。
/// </summary>
public sealed class PleaseProtectMePower : MiyakoModPower
{
    public override PowerType Type => PowerType.Buff;

    // 层数 = 下回合给几层羁绊
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>这一轮有没有掉过血。</summary>
    private bool _tookHpLoss;

    // 图标：按项目命名约定找 tsukiyukimiyako-please_protect_me_power.png，缺图回落通用图标
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

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // 只记"掉血"，回血/被格挡都不算
        if (creature == base.Owner && delta < 0m)
        {
            _tookHpLoss = true;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 只认挂在自己身上的这一位玩家
        if (player != base.Owner.Player)
        {
            return;
        }

        if (!_tookHpLoss && !base.Owner.IsDead)
        {
            Flash();
            await PowerCmd.Apply<SenseiPower>(
                choiceContext,
                base.Owner,
                base.Amount,
                base.Owner,
                null,
                silent: true);
        }

        await PowerCmd.Remove(this);
    }
}
