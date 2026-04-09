using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Powers;

public abstract class MiyakoModPower : CustomPowerModel
{
    // 效果图标（小图）
    public override string CustomPackedIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/powers/power.png";
        }
    }

    // 效果大图
    public override string CustomBigIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/powers/big/power.png";
        }
    }
}