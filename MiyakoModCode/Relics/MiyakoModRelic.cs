using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Relics;

[Pool(typeof(Tsukiyuki_Miyako.MiyakoModCode.Character.MiyakoRelicPool))]
public abstract class MiyakoModRelic : CustomRelicModel
{
    // 遗物小图标
    public override string PackedIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/relics/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/relics/relic.png";
        }
    }

    // 遗物外框
    protected override string PackedIconOutlinePath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/relics/{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/relics/relic_outline.png";
        }
    }

    // 遗物大图
    protected override string BigIconPath
    {
        get
        {
            string path = $"res://Tsukiyuki Miyako/images/relics/big/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://Tsukiyuki Miyako/images/relics/big/relic.png";
        }
    }
}