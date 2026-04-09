using BaseLib.Abstracts;
using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Character;

public class MiyakoCardPool : CustomCardPoolModel
{
    public override string Title => "Miyako";

    // 能量图标路径（正确）
    public override string BigEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/big_energy.png";
    public override string TextEnergyIconPath => "res://Tsukiyuki Miyako/images/charui/text_energy.png";

    // 宫子 主题色 —— 蓝紫色
    public override float H => 0.77f;
    public override float S => 0.40f;
    public override float V => 0.92f;

    // 卡组界面小卡片颜色
    public override Color DeckEntryCardColor => new Color("#73a2ff");

    public override bool IsColorless => false;
}