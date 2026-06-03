using BaseLib.Abstracts;
using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Character;

public class MiyakoCardPool : CustomCardPoolModel
{
    public override string Title => "Miyako";

    public override string BigEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/big_energy.png";
    public override string TextEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/text_energy.png";

    // Miyako theme color — blue-purple (#73a2ff in HSL)
    public override float H => 0.61f;
    public override float S => 0.55f;
    public override float V => 1.00f;

    public override Color DeckEntryCardColor => new Color("#73a2ff");

    public override bool IsColorless => false;
}