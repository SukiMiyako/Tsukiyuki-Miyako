using MegaCrit.Sts2.Core.Models.Cards;
using TsukiyukiMiyako.Scripts;
using TsukiyukiMiyako.Scripts.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class TemporaryPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Power<RestPower>();

    protected override bool IsPositive => true;
}
