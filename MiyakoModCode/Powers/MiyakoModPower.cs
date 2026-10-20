using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Powers;

public abstract class MiyakoModPower : CustomPowerModel
{
    // 效果图标（小图）
    public override string CustomPackedIconPath => FindIcon("powers");

    // 效果大图
    public override string CustomBigIconPath => FindIcon("powers/big");

    /// <summary>
    /// 按两种命名找图标，都找不到才回落通用图。
    ///
    /// 为什么要找两遍：项目里的 power 图标习惯带 mod 前缀
    /// （`tsukiyukimiyako-rabbit_fang_power.png` —— 绝大多数 power 自己写路径时用的是
    /// `Id.Entry.ToLowerInvariant()`），而这个基类原来只按 `RemovePrefix()` 找**无前缀**的名字
    /// （`rabbit_fang_power.png`）。于是没自己写路径的 power（寂寞 / 下回合装填 / 一步不退 /
    /// 小队支援：咲 / 临时力量）就算图画好了也永远命中不到，静默回落成通用 `power.png`。
    /// 现在两种都认，谁先存在用谁，以后加 power 不用再纠结要不要写 override。
    /// </summary>
    private string FindIcon(string folder)
    {
        string entry = Id.Entry.ToLowerInvariant();                 // tsukiyukimiyako-xxx_power
        string bare = Id.Entry.RemovePrefix().ToLowerInvariant();   // xxx_power
        string[] candidates =
        [
            $"res://Tsukiyuki Miyako/images/{folder}/{entry}.png",
            $"res://Tsukiyuki Miyako/images/{folder}/{bare}.png"
        ];
        foreach (string path in candidates)
        {
            if (ResourceLoader.Exists(path))
            {
                return path;
            }
        }
        return $"res://Tsukiyuki Miyako/images/{folder}/power.png";
    }
}