using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TsukiyukiMiyako.Scripts;

public sealed class ShiningRabbitPower : CustomPowerModel
{
    // 出牌计数（照搬你的参考代码）
    private class Data
    {
        public int CardsPlayedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override object InitInternalData() => new Data();

    // 回合开始重置计数（照搬你的参考）
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side == base.Owner.Side)
        {
            GetInternalData<Data>().CardsPlayedThisTurn = 0;
        }
        return Task.CompletedTask;
    }

    // =====================
    // 【严格复刻 MasterPlannerPower 出牌监听】
    // 格式、判断、写法完全一致
    // =====================
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 原版判断：只监听自己的牌
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return;
        }

        var data = GetInternalData<Data>();
        // 判断：当前是本回合前N张牌（基础1张，升级2张）
        if (data.CardsPlayedThisTurn < base.Amount)
        {
            Flash();
            // 官方升级卡牌（照搬Armaments）
            CardCmd.Upgrade(cardPlay.Card);
        }

        // 计数+1
        data.CardsPlayedThisTurn++;
    }
}