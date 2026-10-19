using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;


namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// 「省着点用」——**每回合内**每消耗 <see cref="ExhaustsPerDraw"/> 张牌，抽 1 张牌；
/// **回合结束清零，不跨回合累积**。
///
/// ★ 为什么这么改（实测）：
/// 她的 14 张配备牌**全部带消耗**，原来"消耗一张就抽一张"等于
/// 「打配备牌不亏手牌 + 军备箱生成 → 抽 → 再打 → 再抽」的左脚踩右脚，
/// 消耗这个代价被彻底抹掉。现在一回合内要先消耗满 3 张才兑现一次，
/// 而且余数不带到下回合 —— 想靠它滚雪球就得在同一回合真的把牌烧掉。
///
/// 计数在**玩家回合开始时清零**（`AfterPlayerTurnStart`），而不是回合结束时清，
/// 这样回合结束阶段消耗的虚无牌仍算在当回合的账上，不会被提前抹掉。
///
/// Amount = 每次兑现时抽几张（默认 1）。
/// </summary>
public sealed class EconomizePower : CustomPowerModel
{
    /// <summary>每回合内攒够几张消耗才兑现一次抽牌。</summary>
    public const int ExhaustsPerDraw = 3;

    private class Data
    {
        /// <summary>本回合已经消耗掉的张数。</summary>
        public int exhaustCount;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };

    protected override object InitInternalData()
    {
        return new Data();
    }

    /// <summary>回合开始清零：余数不带进下一回合。</summary>
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == base.Owner)
        {
            GetInternalData<Data>().exhaustCount = 0;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature != base.Owner)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.exhaustCount++;
        if (data.exhaustCount < ExhaustsPerDraw)
        {
            return;
        }

        // 兑现一次（一次消耗 6 张的情况由调用方循环触发，这里只需把余数减掉）
        data.exhaustCount -= ExhaustsPerDraw;
        await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player!);
    }
}
