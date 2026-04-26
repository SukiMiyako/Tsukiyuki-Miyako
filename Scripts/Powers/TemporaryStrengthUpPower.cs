using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using TsukiyukiMiyako.Scripts.Cards;

namespace TsukiyukiMiyako.Scripts.Powers;

public class TemporaryStrengthUpPower : TemporaryStrengthPower
{
    // 来源绑定你的 Rest 卡牌
    public override AbstractModel OriginModel => ModelDb.Card<Rest>();

    // 正面增益效果
    protected override bool IsPositive => true;
}