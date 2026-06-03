using BaseLib.Abstracts;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Character;

public class MiyakoPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => MiyakoMod.Color;

    public override string BigEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/big_energy.png";
    public override string TextEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/text_energy.png";
}