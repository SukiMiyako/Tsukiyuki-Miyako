using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using MegaCrit.Sts2.Core.Entities.Players;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Commands;



namespace TsukiyukiMiyako.Scripts.Relics;

[Pool(typeof(MiyakoRelicPool))]
public class OrigDream : CustomRelicModel
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Common;

    // 遗物的数值。替换本地化中的{Cards}。
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(1),
        new StarsVar(1),
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
            await PowerCmd.Apply<SenseiPower>(base.Owner.Creature, base.DynamicVars["SenseiPower"].BaseValue, base.Owner.Creature, null);
            await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue, base.Owner);
        }
    }


    // 小图标
    public override string PackedIconPath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
    // 轮廓图标
    protected override string PackedIconOutlinePath => "res://Tsukiyuki Miyako/images/relics/orig_dream.png";
    // 大图标
    protected override string BigIconPath => "res://Tsukiyuki Miyako/images/relics/orid_dream.png";
}