using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;
using TsukiyukiMiyako.Scripts.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;

namespace TsukiyukiMiyako.Scripts.Relics;

[Pool(typeof(MiyakoRelicPool))]
public class OrigDreamUpgraded : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(1),
        new StarsVar(2),
        new DynamicVar("SenseiPower", 2)
    };

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner)
        {
            return count;
        }
        return count + base.DynamicVars.Cards.BaseValue;
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();
            await PowerCmd.Apply<SenseiPower>(new BlockingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars["SenseiPower"].BaseValue, base.Owner.Creature, null);
            await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue, base.Owner);
            await PowerCmd.Apply<DreamNameplatePower>(new BlockingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null);
        }
    }

    public override string PackedIconPath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
    protected override string PackedIconOutlinePath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
    protected override string BigIconPath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
}