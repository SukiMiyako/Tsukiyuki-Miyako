using BaseLib.Abstracts;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Character;

public class MiyakoRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => MiyakoMod.Color;

    //绝对正确路径，不用扩展方法，100%不报错
    public override string BigEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/big_energy.png";
    public override string TextEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/text_energy.png";
}