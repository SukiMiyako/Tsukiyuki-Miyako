using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts;

public sealed class MostSpecialTeamPower : CustomPowerModel
{
    // ===================== 修复：定义 Data 类，包含 GeneratedCards =====================
    private class Data
    {
        // 必须显式定义 GeneratedCards
        public List<CardModel> GeneratedCards { get; set; } = new List<CardModel>();
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override object InitInternalData()
    {
        // 初始化 Data
        return new Data();
    }

    // 【复刻 ReadyOrNot】生成小队支援牌 + 免费
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (player != base.Owner.Player)
            return;

        var data = GetInternalData<Data>();
        data.GeneratedCards.Clear(); // 清空上回合数据

        // 随机生成 1 张小队支援牌
        CardModel card = CardFactory.GetDistinctForCombat(player,
            from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            where c.CanonicalKeywords.Contains(MyKeywords.Support)
            select c, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault()!;

        if (card != null)
        {
            card.SetToFreeThisTurn(); // 本回合免费
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
            data.GeneratedCards.Add(card); // 记录临时牌
        }
    }

    // ===================== 修复：100% 照搬 LaserPointer 格式 =====================
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == base.Owner.Side)
        {
            var data = GetInternalData<Data>();

            // 移除本回合生成的所有临时卡牌
            foreach (var card in data.GeneratedCards)
            {
                if (card != null)
                {
                    // ===================== 关键修复：使用 RemoveFromPile（从当前卡组移除） =====================
                    await CardPileCmd.RemoveFromCombat(card);
                }
            }

            data.GeneratedCards.Clear(); // 清空记录
        }
    }
}