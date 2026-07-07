# 杀戮尖塔 2 自定义角色 Mod 从零搭建指南

> 本文档基于 [Tsukiyuki Miyako](https://github.com/SukiMiyako) 项目的实际结构编写，旨在帮助你在 30 分钟内搭好一个可运行的 STS2 自定义角色 Mod 骨架。

---

## 目录

1. [前置准备](#1-前置准备)
2. [项目初始化](#2-项目初始化)
3. [Mod 元数据](#3-mod-元数据)
4. [入口文件](#4-入口文件)
5. [角色定义](#5-角色定义)
6. [卡池 & 遗物池 & 药水池](#6-卡池--遗物池--药水池)
7. [基础类（可选但推荐）](#7-基础类可选但推荐)
8. [第一张卡牌](#8-第一张卡牌)
9. [第一个能力](#9-第一个能力)
10. [自定义关键词](#10-自定义关键词)
11. [本地化文件](#11-本地化文件)
12. [Godot 场景与资源](#12-godot-场景与资源)
13. [构建 & 测试](#13-构建--测试)
14. [附录：完整文件清单](#14-附录完整文件清单)

---

## 1. 前置准备

| 工具 | 版本要求 | 说明 |
|------|---------|------|
| **Slay the Spire 2** | 最新 | Steam 安装 |
| **Godot Engine** | 4.5.1+ (.NET 版) | 用于 UI/场景/动画 |
| **.NET SDK** | 9.0 | `dotnet --version` 确认 |
| **BaseLib** | ≥ 3.3.2 | 社区 Mod 框架，[GitHub](https://github.com/Alchyr/BaseLib-StS2) |

安装 BaseLib：将 `BaseLib.dll` 放到 `<STS2>/mods/BaseLib/` 目录下。

---

## 2. 项目初始化

### 2.1 创建 Godot 项目

在 Godot 中新建项目，**必须勾选 .NET 支持**。项目名就是你的 Mod 名，例如 `MyCharacter`。

### 2.2 配置 .csproj

编辑 `.csproj` 文件，关键配置如下：

```xml
<Project Sdk="Godot.NET.Sdk/4.5.1">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>true</ImplicitUsings>
    <LangVersion>12.0</LangVersion>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>

    <!-- 改成你的 STS2 安装目录 -->
    <Sts2Dir>D:\Steam\steamapps\common\Slay the Spire 2</Sts2Dir>
    <Sts2DataDir>$(Sts2Dir)\data_sts2_windows_x86_64</Sts2DataDir>
  </PropertyGroup>

  <ItemGroup>
    <!-- 游戏本体 -->
    <Reference Include="sts2">
      <HintPath>$(Sts2DataDir)\sts2.dll</HintPath>
      <Private>false</Private>
    </Reference>

    <!-- Harmony 运行时补丁 -->
    <Reference Include="0Harmony">
      <HintPath>$(Sts2DataDir)\0Harmony.dll</HintPath>
      <Private>false</Private>
    </Reference>

    <!-- BaseLib 社区框架 -->
    <Reference Include="BaseLib">
      <HintPath>$(Sts2Dir)/mods/BaseLib/BaseLib.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <!-- 构建后自动复制到 mods 目录 -->
  <Target Name="Copy Mod" AfterTargets="PostBuildEvent">
    <Message Text="Copying mod to Slay the Spire 2 mods folder..." Importance="high" />
    <MakeDir Directories="$(Sts2Dir)\mods\$(MSBuildProjectName)\" />
    <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(Sts2Dir)\mods\$(MSBuildProjectName)\" />
    <Copy SourceFiles="$(MSBuildProjectName).json" DestinationFolder="$(Sts2Dir)/mods/$(MSBuildProjectName)/" />
  </Target>
</Project>
```

> **注意**：`<Sts2Dir>` 路径按你的实际 Steam 库位置修改。`$(MSBuildProjectName)` 会自动取你的项目名，所以项目名要和 Mod 名一致。

### 2.3 创建目录结构

```
你的项目/
├── Scripts/                    # 卡牌、能力、遗物等游戏逻辑
│   ├── Cards/                  # 卡牌 .cs 文件
│   ├── Powers/                 # 能力 .cs 文件
│   ├── Relics/                 # 遗物 .cs 文件
│   ├── Entry.cs                # Mod 入口
│   └── MyKeywords.cs           # 自定义关键词
├── MyModCode/                  # 基础类 & 角色定义（与 Scripts 分开）
│   ├── Character/              # 角色模型、卡池/遗物池/药水池
│   ├── Cards/                  # 卡牌基类
│   ├── Powers/                 # 能力基类
│   ├── Relics/                 # 遗物基类
│   └── Nodes/                  # 自定义 Godot 节点（可选）
├── 你的项目名/                 # Godot 资源目录（与项目名一致）
│   ├── scenes/                 # 角色动画 .tscn、UI 场景
│   ├── images/                 # 卡图、图标
│   └── localization/zhs/       # 简体中文（或其他语言）
├── 你的项目名.csproj
└── 你的项目名.json             # Mod 元数据
```

> **命名约定**：
> - `Scripts/` 下放具体实现，命名空间用 `YourProject.Scripts`
> - `MyModCode/` 下放抽象基类和角色基础设施，命名空间用 `YourProject.MyModCode`（建议把 `My` 替换成角色名）
> - `你的项目名/` 目录是 Godot 的资源根目录，里面的 `scenes/`、`images/` 等路径对应 `res://` 开头的 Godot 资源路径

---

## 3. Mod 元数据

项目根目录下的 `<项目名>.json`：

```json
{
    "id": "Your Character Name",
    "name": "Your Character Name",
    "author": "你的名字",
    "description": "添加新角色「XXX」— 简短描述。",
    "version": "0.1.0",
    "has_pck": true,
    "has_dll": true,
    "dependencies": [
        {
            "id": "BaseLib",
            "minVersion": "3.3.2"
        }
    ],
    "affects_gameplay": true
}
```

- `id` 和 `name` 保持一致即可
- `has_pck: true` 表示附带 Godot 打包资源
- `has_dll: true` 表示附带 C# DLL
- `dependencies` 必须声明 BaseLib

---

## 4. 入口文件

`Scripts/Entry.cs` —— Mod 加载时最先执行的文件：

```csharp
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace YourProject.Scripts;

/// <summary>
/// Mod entry point. 方法名要和 [ModInitializer] 参数一致。
/// </summary>
[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        // 1. 应用 Harmony 补丁（即使现在没有 patch，也要创建实例）
        var harmony = new Harmony("sts2.yourname.charactername");
        harmony.PatchAll();

        // 2. 注册自定义 Godot 脚本
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);

        Log.Debug("Mod initialized!");
    }
}
```

> **关键点**：
> - `[ModInitializer("Init")]` 的参数必须与方法名匹配
> - Harmony ID 使用唯一字符串，推荐格式：`sts2.<作者>.<角色名>`
> - `ScriptManagerBridge.LookupScriptsInAssembly` 是必须的，否则 `.tscn` 场景无法解析 C# 类型

---

## 5. 角色定义

`MyModCode/Character/MyCharacterMod.cs`：

```csharp
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using YourProject.Scripts.Cards;
using YourProject.Scripts.Relics;
using YourProject.MyModCode.Character;

namespace YourProject.MyModCode.Character;

public class MyCharacterMod : PlaceholderCharacterModel
{
    public const string CharacterId = "MyCharacter";

    public static readonly Color Color = new("#你的主题色");

    // ===== 基础属性 =====
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;  // 初始血量

    // ===== 自定义场景路径（角色动画、休息点、商店等）=====
    public override string CustomVisualPath =>
        "res://YourProject/scenes/creature_visuals/your_character.tscn";
    public override string CustomRestSiteAnimPath =>
        "res://YourProject/scenes/rest_site/your_character_rest_site.tscn";
    public override string CustomMerchantAnimPath =>
        "res://YourProject/scenes/merchant/your_character_merchant.tscn";
    public override string CustomEnergyCounterPath =>
        "res://YourProject/scenes/energy/your_energy_counter.tscn";
    public override string CustomIconTexturePath =>
        "res://your_icon.svg";
    public override string CustomIconPath =>
        "res://YourProject/scenes/ui/character_icons/your_icon.tscn";

    // ===== 角色选择界面 =====
    public override string CustomCharacterSelectIconPath =>
        "res://YourProject/images/charui/char_select_char_name.png";
    public override string CustomCharacterSelectLockedIconPath =>
        "res://YourProject/images/charui/char_select_char_name.png";
    public override string CustomCharacterSelectBg =>
        "res://YourProject/images/charui/char_select_bg.tscn";

    // ===== 池注册 =====
    public override CardPoolModel CardPool => ModelDb.CardPool<MyCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<MyRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<MyPotionPool>();

    // ===== 初始遗物 =====
    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<MyStartingRelic>()
    ];

    // ===== 初始卡组 =====
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<Strike>(),
        ModelDb.Card<Strike>(),
        ModelDb.Card<Strike>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
    ];

    // ===== 攻击特效池（选填）=====
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
    ];
}
```

> **提示**：如果你暂时没有做自定义场景/动画，可以把 `CustomVisualPath` 等路径注释掉或指向占位资源。最简启动可以先只保留基础属性 + 池 + 初始卡组。

---

## 6. 卡池 & 遗物池 & 药水池

这三个池决定了角色能拿到哪些卡牌、遗物、药水。

### 6.1 卡池

`MyModCode/Character/MyCardPool.cs`：

```csharp
using BaseLib.Abstracts;
using Godot;

namespace YourProject.MyModCode.Character;

public class MyCardPool : CustomCardPoolModel
{
    public override string Title => "MyCharacter";

    // 能量图标（卡牌描述中显示）
    public override string BigEnergyIconPath =>
        "res://YourProject/images/charui/big_energy.png";
    public override string TextEnergyIconPath =>
        "res://YourProject/images/charui/text_energy.png";

    // 主题色（HSL）
    public override float H => 0.61f;  // 色相 0~1
    public override float S => 0.55f;  // 饱和度
    public override float V => 1.00f;  // 明度

    public override Color DeckEntryCardColor => new Color("#你的主题色");

    public override bool IsColorless => false;
}
```

### 6.2 遗物池

`MyModCode/Character/MyRelicPool.cs`：

```csharp
using BaseLib.Abstracts;
using Godot;

namespace YourProject.MyModCode.Character;

public class MyRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => MyCharacterMod.Color;

    public override string BigEnergyIconPath =>
        "res://YourProject/images/charui/big_energy.png";
    public override string TextEnergyIconPath =>
        "res://YourProject/images/charui/text_energy.png";
}
```

### 6.3 药水池

`MyModCode/Character/MyPotionPool.cs`：

```csharp
using BaseLib.Abstracts;

namespace YourProject.MyModCode.Character;

public class MyPotionPool : CustomPotionPoolModel
{
}
```

---

## 7. 基础类（可选但推荐）

这些抽象类提供自动图标/卡图路径回退，减少重复代码。

### 7.1 卡牌基类

`MyModCode/Cards/MyModCard.cs`：

```csharp
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using YourProject.MyModCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace YourProject.MyModCode.Cards;

[Pool(typeof(MyCardPool))]
public abstract class MyModCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    // 大图（卡牌详情界面）
    public override string CustomPortraitPath =>
        $"res://YourProject/images/card_portraits/big/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    // 小图（战斗/卡组界面）
    public override string PortraitPath =>
        $"res://YourProject/images/card_portraits/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    // Beta 图
    public override string BetaPortraitPath =>
        $"res://YourProject/images/card_portraits/beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
}
```

> 继承 `MyModCard` 后，只需把 PNG 放到对应目录，路径自动匹配类名。不用每张卡手写 `PortraitPath`。

### 7.2 能力基类

`MyModCode/Powers/MyModPower.cs`：

```csharp
using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;

namespace YourProject.MyModCode.Powers;

public abstract class MyModPower : CustomPowerModel
{
    // 小图标
    public override string CustomPackedIconPath
    {
        get
        {
            string path = $"res://YourProject/images/powers/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://YourProject/images/powers/power.png";
        }
    }

    // 大图标
    public override string CustomBigIconPath
    {
        get
        {
            string path = $"res://YourProject/images/powers/big/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://YourProject/images/powers/big/power.png";
        }
    }
}
```

### 7.3 遗物基类

`MyModCode/Relics/MyModRelic.cs`：

```csharp
using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;

namespace YourProject.MyModCode.Relics;

public abstract class MyModRelic : CustomRelicModel
{
    public override string CustomPackedIconPath
    {
        get
        {
            string path = $"res://YourProject/images/relics/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://YourProject/images/relics/relic.png";
        }
    }

    public override string CustomOutlinePath
    {
        get
        {
            string path = $"res://YourProject/images/relics/outline/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://YourProject/images/relics/outline/relic.png";
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            string path = $"res://YourProject/images/relics/big/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            return ResourceLoader.Exists(path) ? path : "res://YourProject/images/relics/big/relic.png";
        }
    }
}
```

---

## 8. 第一张卡牌

以打击（Strike）为例，`Scripts/Cards/Strike.cs`：

```csharp
using BaseLib.Abstracts;
using BaseLib.Utils;
using YourProject.MyModCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace YourProject.Scripts.Cards;

[Pool(typeof(MyCardPool))]   // 注册到角色卡池
public class Strike : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    // 基础数值：6 点伤害，Move 表示可被力量等 buff 修正
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move)
    ];

    // 卡图
    public override string PortraitPath =>
        $"res://YourProject/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public Strike() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    // 打出效果
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    // 标签
    protected override HashSet<CardTag> CanonicalTags =>
        new HashSet<CardTag> { CardTag.Strike };

    // 升级
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
```

### 卡牌关键概念速查

| 概念 | 说明 | 示例 |
|------|------|------|
| `CanonicalVars` | 卡牌的基础数值（伤害/格挡/魔法数等） | `new DamageVar(6m, ValueProp.Move)` |
| `DynamicVars.Damage` | 引用 CanonicalVars 中定义的伤害值 | `DynamicVars.Damage.BaseValue` |
| `ValueProp.Move` | 值可被力量等 buff 修正 | 几乎所有攻击/格挡都用 |
| `ValueProp.Flat` | 固定值，不受 buff 影响 | 固定扣血等 |
| `CardTag.Strike` | 打击标签（被完美打击等卡牌检测） | 打击类都要加 |
| `CardTag.Defend` | 防御标签 | 防御类都要加 |
| `DamageCmd.Attack()` | 造成伤害的命令 | `Attack(value).FromCard(this).Targeting(target).Execute(ctx)` |
| `CreatureCmd.GainBlock()` | 获得格挡 | `GainBlock(owner, amount, cardPlay)` |
| `PlayerCmd.GainStars()` | 获得"星"（第二资源） | `GainStars(amount, player)` |
| `UpgradeValueBy(n)` | 升级时增加 n 点数值 | `.UpgradeValueBy(3)` |

### 命令模式常用 API

```csharp
// 单体伤害
await DamageCmd.Attack(value).FromCard(this).Targeting(target).Execute(ctx);

// AoE 伤害（对所有敌人）
await DamageCmd.Attack(value).FromCard(this)
    .TargetingAllOpponents(ctx.State).Execute(ctx);

// 多段伤害
await DamageCmd.Attack(value).FromCard(this).Targeting(target)
    .WithHitCount(3).Execute(ctx);

// 格挡
await CreatureCmd.GainBlock(owner.Creature, blockAmount, cardPlay);

// 抽牌
await CardCmd.DrawCards(amount, ctx);

// 获得能量
await PlayerCmd.GainEnergy(amount, ctx.Player);

// 施加能力（buff/debuff）
await PowerCmd.Apply<MyPower>(ctx, target, stacks, applier, cardSource);
```

---

## 9. 第一个能力

`Scripts/Powers/MyFirstPower.cs`：

```csharp
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YourProject.MyModCode.Powers;

namespace YourProject.Scripts.Powers;

public sealed class MyFirstPower : MyModPower
{
    // Buff 还是 Debuff
    public override PowerType Type => PowerType.Buff;

    // 叠加方式：Counter（数字叠加）、Intensity（强度叠加）、Independent（独立实例）
    public override PowerStackType StackType => PowerStackType.Counter;

    // 是否允许负数
    public override bool AllowNegative => false;

    // ═══ 以下是各种生命周期钩子，按需 override ═══

    // 回合开始时
    public override async Task AtStartOfTurn(PlayerChoiceContext ctx)
    {
        // 例如：回合开始时获得 1 点力量
        // await PowerCmd.Apply<StrengthPower>(ctx, base.Owner, 1, base.Owner, null);
    }

    // 能量回复后
    public override async Task AfterEnergyReset(PlayerChoiceContext ctx)
    {
    }

    // 打出卡牌时
    public override async Task OnCardPlayed(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
    }

    // 受到伤害时
    public override async Task<int> OnAttacked(PlayerChoiceContext ctx, Creature attacker,
        int damage, DamageType damageType, CardModel? cardSource)
    {
        return damage; // 返回修改后的伤害值
    }

    // 造成伤害时
    public override async Task<int> OnAttack(PlayerChoiceContext ctx, Creature target,
        int damage, DamageType damageType, CardModel? cardSource)
    {
        return damage;
    }

    // 能力层数变化后
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext ctx, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
    }
}
```

### Power 常用钩子

| 钩子 | 触发时机 | 返回类型 |
|------|---------|---------|
| `AtStartOfTurn` | 回合开始 | `Task` |
| `AfterEnergyReset` | 能量回复后 | `Task` |
| `OnCardPlayed` | 打出任意卡牌 | `Task` |
| `OnAttacked` | 受到攻击伤害 | `Task<int>`（返回实际伤害） |
| `OnAttack` | 造成攻击伤害 | `Task<int>`（返回实际伤害） |
| `AfterPowerAmountChanged` | 本能力层数变化后 | `Task` |
| `BeforeApplied` | 本能力被施加前 | `Task` |
| `OnRemove` | 本能力被移除时 | `Task` |

---

## 10. 自定义关键词

`Scripts/MyKeywords.cs`：

```csharp
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace YourProject.Scripts;

public class MyKeywords
{
    [CustomEnum("MY_KEYWORD_1")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword MyKeyword1;

    [CustomEnum("MY_KEYWORD_2")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword MyKeyword2;
}
```

- `[CustomEnum("KEY")]` 注册关键词枚举值
- `[KeywordProperties]` 控制关键词标签的显示位置（`Before` 描述 / `After` 描述）
- 卡牌通过 `CanonicalTags` 引用关键词：
  ```csharp
  protected override HashSet<CardTag> CanonicalTags => new() { MyKeywords.MyKeyword1 };
  ```

---

## 11. 本地化文件

所有文本都走本地化 JSON，**不要硬编码中文到 C# 代码中**。

目录结构：
```
YourProject/localization/zhs/
├── cards.json
├── powers.json
├── relics.json
├── orbs.json
├── characters.json
├── card_keywords.json
└── static_hover_tips.json
```

### 11.1 卡牌本地化 (`cards.json`)

```json
{
  "YOURPROJECT-STRIKE.title": "打击",
  "YOURPROJECT-STRIKE.description": "造成{Damage:diff()}点伤害。",

  "YOURPROJECT-DEFEND.title": "防御",
  "YOURPROJECT-DEFEND.description": "获得{Block:diff()}点格挡。"
}
```

**Key 格式**：`MODPREFIX-CLASSNAME.title` / `MODPREFIX-CLASSNAME.description`
- `MODPREFIX` = 你的项目名全大写（如 `TSUKIYUKIMIYAKO`）
- `CLASSNAME` = C# 类名全大写（如 `STRIKE`）

### 11.2 动态变量格式

| 格式 | 渲染结果 |
|------|---------|
| `{Damage:diff()}` | 伤害值，含升级变化（绿色高亮） |
| `{Block:diff()}` | 格挡值，含升级变化 |
| `{Power:diff()}` | 第一个 Power 型 CanonicalVar |
| `{Stars:starIcons()}` | 星/弹夹图标 |
| `{Energy:energyIcons()}` | 能量图标 |
| `{Cards:diff()}` | 卡牌数量 |

### 11.3 角色本地化 (`characters.json`)

```json
{
  "YOURPROJECT-MYCHARACTERMOD.title": "你的角色名",
  "YOURPROJECT-MYCHARACTERMOD.description": "角色的简短描述。",
  "YOURPROJECT-MYCHARACTERMOD.pronounSubject": "她",
  "YOURPROJECT-MYCHARACTERMOD.pronounObject": "她",
  "YOURPROJECT-MYCHARACTERMOD.pronounPossessive": "她的",
  "YOURPROJECT-MYCHARACTERMOD.possessiveAdjective": "她的"
}
```

> Key 中的类名部分要和角色定义类名一致（全大写），注意下划线：`MY_CHARACTER_MOD` 对应类名 `MyCharacterMod`。

### 11.4 能力本地化 (`powers.json`)

```json
{
  "YOURPROJECT-MYFIRSTPOWER.title": "我的第一个能力",
  "YOURPROJECT-MYFIRSTPOWER.description": "回合开始时获得1点力量。"
}
```

---

## 12. Godot 场景与资源

### 12.1 场景文件参考

| 路径 | 用途 |
|------|------|
| `scenes/creature_visuals/xxx.tscn` | 战斗中的角色 Spine 动画 |
| `scenes/rest_site/xxx_rest_site.tscn` | 休息点动画 |
| `scenes/merchant/xxx_merchant.tscn` | 商店动画 |
| `scenes/energy/xxx_energy_counter.tscn` | 能量球 UI 组件 |
| `scenes/ui/character_icons/xxx.tscn` | 角色选择界面图标 |

如果你还没有自定义动画，可以先使用 Godot 的占位 sprite 或直接省略这些路径。角色在战斗中会显示为默认外观。

### 12.2 图片资源目录

```
YourProject/images/
├── cards/                        # 卡牌小图（战斗中/卡组列表）
│   └── strike.png
├── card_portraits/               # 卡牌竖版大图
│   ├── big/                      # 详情界面大图
│   ├── beta/                     # Beta 版卡图
│   └── strike.png                # 普通版
├── powers/                       # 能力图标
│   ├── big/                      # 大图标
│   └── power.png                 # 默认回退图标
├── relics/                       # 遗物图标
│   ├── big/
│   ├── outline/
│   └── relic.png                 # 默认回退图标
└── charui/                       # 角色 UI 资源
    ├── big_energy.png            # 大能量图标
    ├── text_energy.png           # 文本中嵌入的能量图标
    ├── char_select_char_name.png # 角色选择界面的角色图标
    └── char_select_bg.tscn       # 角色选择界面的背景
```

---

## 13. 构建 & 测试

### 13.1 构建

```bash
# 在项目根目录
dotnet build
```

构建成功后，DLL 和 JSON 会自动复制到 `<STS2>/mods/你的项目名/`。

### 13.2 在游戏中测试

1. 启动 STS2
2. 主菜单 → Mod → 勾选你的 Mod
3. 开始新游戏，检查角色选择界面是否出现你的角色
4. 进入战斗，验证初始卡组和基础逻辑

### 13.3 调试技巧

- 游戏日志在 `<STS2>/Logs/` 目录下
- C# 中用 `Log.Debug("...")`、`Log.Warning("...")`、`Log.Error("...")` 输出日志
- 修改 DLL 后需要在游戏中重新加载 Mod（或重启游戏）

### 13.4 常见启动问题

| 问题 | 可能原因 |
|------|---------|
| 角色不显示在选择界面 | 检查 `Entry.cs` 是否被 `[ModInitializer]` 正确标记；检查 Godot 场景路径是否存在 |
| 卡牌无文字/图标 | 检查本地化 JSON 的 key 格式；检查图片文件路径和命名 |
| DLL 加载失败 | 检查 `.csproj` 中的引用路径是否正确；检查 BaseLib 版本 |
| Harmony 报错 | 检查 Harmony ID 是否与其他 Mod 冲突 |

---

## 14. 附录：完整文件清单

以下是初始化一个最小可运行 Mod 需要的全部文件（不含美术资源时）：

```
YourProject/
├── YourProject.csproj                    # 项目文件（含 STS2 路径和自动复制）
├── YourProject.json                      # Mod 元数据
├── Scripts/
│   ├── Entry.cs                          # Mod 入口
│   ├── MyKeywords.cs                     # 自定义关键词（可选，无关键词可省略）
│   ├── Cards/
│   │   ├── Strike.cs                     # 打击
│   │   └── Defend.cs                     # 防御
│   ├── Powers/
│   │   └── MyFirstPower.cs               # 示例能力
│   └── Relics/
│       └── MyStartingRelic.cs            # 初始遗物
├── MyModCode/
│   ├── Character/
│   │   ├── MyCharacterMod.cs             # 角色定义
│   │   ├── MyCardPool.cs                 # 卡池
│   │   ├── MyRelicPool.cs                # 遗物池
│   │   └── MyPotionPool.cs               # 药水池
│   ├── Cards/
│   │   └── MyModCard.cs                  # 卡牌基类
│   ├── Powers/
│   │   └── MyModPower.cs                 # 能力基类
│   └── Relics/
│       └── MyModRelic.cs                 # 遗物基类
└── YourProject/
    └── localization/
        └── zhs/
            ├── cards.json                # 卡牌本地化
            ├── powers.json               # 能力本地化
            ├── relics.json               # 遗物本地化
            ├── characters.json           # 角色本地化
            └── card_keywords.json        # 关键词本地化
```

---

## 下一步

1. **先跑通骨架**：完成以上最小文件集，`dotnet build` 不报错，游戏中能看到角色。
2. **加初始卡组**：从 Strike/Defend 开始，逐步添加你的专属机制卡牌。
3. **加能力系统**：设计角色的核心 Buff/Debuff。
4. **加遗物**：专属遗物是角色特色的重要部分。
5. **做美术**：角色 Spine 动画、卡图、图标。

---

> 🤖 本文档基于 [Tsukiyuki Miyako](https://github.com/SukiMiyako) 项目自动生成，结合 CLAUDE.md 和实际源码结构编写。如有问题，欢迎提 Issue。
