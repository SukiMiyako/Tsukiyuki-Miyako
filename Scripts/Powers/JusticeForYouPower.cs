using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TsukiyukiMiyako.Scripts.Cards;

namespace TsukiyukiMiyako.Scripts.Powers;

public sealed class JusticeForYouPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 你的标准图标路径（完全不动）
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    // =====================
    // 【完全照搬 IterationPower】抽牌后触发
    // 方法名、参数、结构 100% 一致
    // =====================
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 原版判断：自己的牌 + 状态牌
        if (card.Owner.Creature == base.Owner && card.Type == CardType.Status)
        {
            // 核心条件：师生羁绊 > 5
            int senseiCount = base.Owner.GetPowerAmount<SenseiPower>();
            if (senseiCount <= 5)
                return;

            Flash();
            // 直接转换为 SRT的正义（无升级，无报错）
            await CardCmd.TransformTo<SrtJustice>(card);
        }
    }
}