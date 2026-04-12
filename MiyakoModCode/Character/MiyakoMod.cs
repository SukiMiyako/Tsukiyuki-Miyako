using BaseLib.Abstracts;
using TsukiyukiMiyako.Scripts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts.Cards;
using TsukiyukiMiyako.Scripts.Relics;

namespace Tsukiyuki_Miyako.MiyakoModCode.Character;

public class MiyakoMod : PlaceholderCharacterModel
{
    public const string CharacterId = "Miyako";

    public static readonly Color Color = new("#73a2ff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;
    public override string CustomVisualPath => "res://Tsukiyuki Miyako/scenes/creature_visuals/tsukiyuki_miyako.tscn";
    public override string CustomRestSiteAnimPath => "res://Tsukiyuki Miyako/scenes/rest_site/tsukiyuki_miyako_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://Tsukiyuki Miyako/scenes/merchant/tsukiyuki_miyako_merchant.tscn";
    public override string CustomEnergyCounterPath => "res://Tsukiyuki Miyako/scenes/energy/miyako_energy_counter.tscn";
    public override string CustomIconTexturePath => "res://miyako.svg";
    public override string CustomIconPath => "res://Tsukiyuki Miyako/scenes/ui/character_icons/miyako_icon.tscn";

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<OrigDream>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<MiyakoCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<MiyakoRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<MiyakoPotionPool>();

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<Strike>(),
        ModelDb.Card<Strike>(),
        ModelDb.Card<Strike>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<SmgSweep>(),
        ModelDb.Card<SmgSweep>(),
        ModelDb.Card<Reload>(),
    ];

    //public override string CustomIconTexturePath => "res://Tsukiyuki Miyako/images/charui/character_icon_char_name.png";
    public override string CustomCharacterSelectIconPath => "res://Tsukiyuki Miyako/images/charui/char_select_char_name.png";
    public override string CustomCharacterSelectLockedIconPath => "res://Tsukiyuki Miyako/images/charui/char_select_char_name.png";
    //public override string CustomMapMarkerPath => "res://Tsukiyuki Miyako/images/charui/character_icon_char_name.png";
    public override string CustomCharacterSelectBg => "res://Tsukiyuki Miyako/images/charui/char_select_bg.tscn";
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}