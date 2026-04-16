using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts;

public sealed class MostReliableCaptainPower : CustomPowerModel
{
    private class Data
    {
        public int EquipmentCardCount;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";
    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    protected override object InitInternalData()
    {
        return new Data();
    }

    // 出牌监听（照搬你的模板）
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        // 判断【配备】标签（照搬你的模板）
        if (cardPlay.Card.CanonicalKeywords.Contains(MyKeywords.Equipment))
        {
            var data = GetInternalData<Data>();
            data.EquipmentCardCount++;

            if (data.EquipmentCardCount >= 3)
            {
                data.EquipmentCardCount = 0;
                Flash();

                // =====================
                // 【严格对标 GainStar 写法】获得弹夹
                // =====================
                await PlayerCmd.GainStars(1, base.Owner.Player);
            }
        }
    }
}