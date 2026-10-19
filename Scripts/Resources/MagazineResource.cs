using BaseLib.Abstracts;
using Godot;
using Tsukiyuki_Miyako.MiyakoModCode.Character;

namespace TsukiyukiMiyako.Scripts.Resources;

/// <summary>
/// 弹夹 —— 宫子自己的战斗内资源，**不复用原版的[gold]星[/gold]**。
///
/// 为什么不再用星：星是**储君（Regent）**的角色资源，任何"给星"的牌/遗物/药水
/// 都会顺手喂饱她，等于白送弹夹。改成自有资源后，这套东西只由她自己产出于她自己的牌。
///
/// 实现完全走 BaseLib 现成的自定义资源框架（照芙莉莲 mod 的 `ManaResource`）：
///   · 继承 <see cref="BasicCustomResource"/>，构造参数第二个是"每回合重置到的值"。
///     **传 -1 = 累积型**：不会每回合补满，靠自己攒、靠换弹牌补。
///   · **不设上限**：`DefaultMax` 保持 -1，所以 `Max` 恒为 0、`GetTextForAmount` 只显示数字。
///     这样 X 费牌（火力倾泻）不会被上限卡住。
///   · 注册是**自动的**：BaseLib 的 `PostModInitPatch` 会反射扫描所有 `CustomResource`
///     子类并调用 `CustomResources<T>.Register`，Entry 里什么都不用写。
///   · UI 也是自动的：`RegisterResourceVisuals` 会注册
///       ① 能量槽旁的常驻资源显示（`NAdditionalResourceDisplay`，位置 `AroundEnergy`）
///       ② 卡面费用旁的资源花费图标（`NAdditionalCostDisplay`，位置 `AroundCost`）
///   · 生命周期：存在 `SpireField<PlayerCombatState, T>` 里，**出战斗即销毁**，
///     不涉及存档，正合"弹夹"的战斗内语义。
///
/// 卡牌侧怎么用：
///   声明花费 → 构造函数里 `CustomResources&lt;MagazineResource&gt;.SetCanonicalCost(this, N)`
///              （X 费牌用 `SetXCost(this)`）
///   产弹夹   → `CustomResources&lt;MagazineResource&gt;.Get(Owner.PlayerCombatState).ModifyAmount(N)`
///   花完做事 → 实现 `IAfterSpendResource&lt;MagazineResource&gt;`（「弹药管够」用它）
///
/// 图标：`images/charui/magazine.png`（70×70，内容已缩到画布 75%，四周留透明边）。
/// **新图导入**：BaseLib 在战斗 UI 里走 `PreloadManager.Cache.GetTexture2D(path)`，
/// 图片必须先被 Godot 导入（生成 `.import`）才能在游戏里显示。
/// </summary>
public sealed class MagazineResource : BasicCustomResource
{
    /// <summary>资源 Id。也用于悬停提示的 key：`static_hover_tips` 里的 `TSUKIYUKIMIYAKO_MAGAZINE.title/.description`。</summary>
    public const string ResourceId = "TSUKIYUKIMIYAKO_MAGAZINE";

    /// <summary>弹夹图标（也用于卡面费用旁的图标与悬停提示）。</summary>
    public const string MagazineTexturePath = "res://Tsukiyuki Miyako/images/charui/magazine.png";

    public override string TexturePath => MagazineTexturePath;

    public override string? IconPath => MagazineTexturePath;

    /// <summary>主题色，弹夹走她角色色系里的浅蓝。</summary>
    public override Color MainColor => new("73a2ff");

    /// <summary>
    /// 第二个参数 -1 = **累积型**：`PrepForCombat` 把 Amount 清 0 后不会每回合自动补满，
    /// 也不会产生上限（`Max` 恒 0，`GetTextForAmount` 只显示当前值）。
    /// </summary>
    public MagazineResource() : base(ResourceId, -1)
    {
    }

    /// <summary>
    /// 资源显示只在**宫子自己**身上出现。
    ///
    /// ★ 兼容性：BaseLib 的自定义资源注册是**全局**的 —— 它给每场战斗的每个玩家都挂一个
    /// 资源显示节点，是否可见最后由 `ShouldShowDisplay()` 决定
    /// （`NAdditionalResourceDisplay.RefreshVisibility`：`Visible ||= _resource.ShouldShowDisplay()`）。
    /// 芙莉莲 mod 的「魔力」直接 `return true`，于是宫子的战斗界面里也会冒出魔力条 ——
    /// 这里必须按角色门禁，否则我们的弹夹同样会泄漏到别的角色身上。
    /// </summary>
    public override bool ShouldShowDisplay() => Owner?.Character is MiyakoMod;
}
