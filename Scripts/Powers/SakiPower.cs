using MegaCrit.Sts2.Core.Models.Cards;
using TsukiyukiMiyako.Scripts.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class SakiPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<SupportSaki>();

    protected override bool IsPositive => false;
}
