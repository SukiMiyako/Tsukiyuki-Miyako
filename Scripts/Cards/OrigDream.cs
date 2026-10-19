using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Resources;
using MegaCrit.Sts2.Core.Entities.Players;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Commands;

namespace TsukiyukiMiyako.Scripts.Relics;

[Pool(typeof(MiyakoRelicPool))]
public class OrigDream : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<OrigDreamUpgraded>();

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(1),
        new DynamicVar("Magazine", 1m),
        new DynamicVar("SenseiPower", 1)
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
            // 修复：
            await PowerCmd.Apply<SenseiPower>(new BlockingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars["SenseiPower"].BaseValue, base.Owner.Creature, null);
            CustomResources<MagazineResource>.Get(Owner.PlayerCombatState!).ModifyAmount(DynamicVars["Magazine"].IntValue);
        }
    }

    public override string PackedIconPath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
    protected override string PackedIconOutlinePath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
    protected override string BigIconPath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
}