---
noteId: "0e9e3fa05e2c11f1b4b5e9cf9c6cc1fe"
tags: []

---

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Working agreement — read first

**分工：人类负责构建，助手只写码 + 扒机制。**

- **不要自己跑 `dotnet build` / `dotnet run` / Godot 导出或导入** —— 包括"只是想确认一下能不能编译"这种情况也不要跑。构建由人类执行，编译错误会贴回来。
- 想确认 API 是否存在、签名对不对，去读反编译源码，而不是编译：
  `D:\slay\_miyako_decomp\sts2src`（游戏本体，命名空间 `MegaCrit.Sts2.Core.*`）。
- 助手的改动范围：`Scripts/Cards/*.cs`、`Scripts/Powers/*.cs`、`MiyakoModCode/*`、`Tsukiyuki Miyako/localization/zhs/*.json`。
- 往游戏 `mods\` 目录拷贝 DLL 失败（`MSB3021`）是**正常现象**，不是 bug —— 游戏运行时占用着 DLL。
- **不要碰这台机器上另外两份 Miyako**（开发版 + Workshop 旧版）。只动本仓库。

## Project overview

A **Slay the Spire 2** mod that adds a custom playable character, **Tsukiyuki Miyako** (月雪宫子, from Blue Archive). Built with Godot 4.5.1 for UI/scenes and C# / .NET 9.0 for game logic. Uses **Harmony** for patching the base game and depends on **BaseLib** as the community modding framework.

## Build & development

> 构建由**人类**执行（见上方 Working agreement）——助手不要运行下面的命令。

```bash
# Build and auto-copy to STS2 mods folder
dotnet build

# The csproj auto-copies the built DLL + mod JSON to:
# D:\Steam\steamapps\common\Slay the Spire 2\mods\Tsukiyuki Miyako\
# Override the path by editing <Sts2Dir> in Tsukiyuki Miyako.csproj
```

There are no tests or lint commands for this project.

## Architecture

### Entry & mod registration

`Scripts/Entry.cs` — the mod entry point marked with `[ModInitializer("Init")]`. On init it:
1. Creates a Harmony instance (`"sts2.sh1ro.miyako"`) and calls `PatchAll()`
2. Registers custom Godot scripts via `ScriptManagerBridge.LookupScriptsInAssembly()`

### Character definition

`MiyakoModCode/Character/MiyakoMod.cs` — extends `PlaceholderCharacterModel`. Defines:
- Character ID `"Miyako"`, base HP 70, theme color `#73a2ff`
- Starting deck, relics, custom visual/animation paths (spine animations loaded from `.tscn` scenes)
- Card/relic/potion pools via `ModelDb.CardPool<>`, `ModelDb.RelicPool<>`, `ModelDb.PotionPool<>`

### Card system

Every card goes in `Scripts/Cards/` and extends `CustomCardModel` (from BaseLib). Most cards have the same pattern:

```csharp
[Pool(typeof(MiyakoCardPool))]               // register to character's card pool
public class MyCard : CustomCardModel
{
    // Constants: energy cost, CardType, CardRarity, TargetType
    // CanonicalVars: damage/block/magic/etc. values
    // PortraitPath: "res://Tsukiyuki Miyako/images/cards/{classname_lower}.png"
    // OnPlay(): async effect logic using command pattern (DamageCmd, CreatureCmd, PlayerCmd, etc.)
    // OnUpgrade(): modify DynamicVars values
}
```

Alternatively, extend `MiyakoModCard` (in `MiyakoModCode/Cards/`) for cards that need auto-resolved custom/big/beta portraits — it provides default portrait path conventions.

Key patterns for card logic:
- `DamageCmd.Attack(value).FromCard(this).Targeting(target).Execute(context)` — deal damage
- `CreatureCmd.GainBlock(owner, amount, cardPlay)` — gain block
- `PlayerCmd.GainStars(amount, player)` — gain "stars" (the character's secondary resource, called 弹夹/magazines)
- `.WithHitCount(n)` — multi-hit attacks
- `.TargetingAllOpponents(state)` — AoE targeting

### Power system

Powers in `Scripts/Powers/` extend `CustomPowerModel`. They define icon paths, `PowerType` (Buff/Debuff), `StackType`, and override hooks like `AfterEnergyReset`, `OnCardPlayed`, etc. Use `MiyakoModPower` base class for auto-fallback icon paths.

### Custom keywords

`Scripts/MyKeywords.cs` — registers custom card keywords (`Equipment`, `Support`, `SMG`) via `[CustomEnum]` and `[KeywordProperties]` attributes. Keywords appear as tags on cards.

### Code-based content outside Scripts/

`MiyakoModCode/` contains abstract base classes and character infrastructure, separated from the card/power implementations:
- `Cards/MiyakoModCard.cs` — optional base class with auto portrait paths
- `Powers/MiyakoModPower.cs` — optional base class with auto icon paths (falls back to default if file missing)
- `Relics/MiyakoModRelic.cs` — base relic with auto icon/outline/big paths
- `Character/MiyakoCardPool.cs`, `MiyakoRelicPool.cs`, `MiyakoPotionPool.cs` — pool definitions
- `Nodes/` — custom Godot nodes (e.g., `SakiEnergyCounter` extends `NEnergyCounter`)

### Localization

JSON files in `Tsukiyuki Miyako/localization/zhs/` (Simplified Chinese):
- `cards.json`, `powers.json`, `relics.json`, `orbs.json`, `characters.json`, `card_keywords.json`, `ancients.json`, `static_hover_tips.json`

Key format: `TSUKIYUKIMIYAKO-CLASSNAME.title` and `TSUKIYUKIMIYAKO-CLASSNAME.description`. The mod prefix is always uppercase; class name matches the C# class name in uppercase. Dynamic variables use format strings like `{Damage:diff()}`, `{Block:diff()}`, `{Stars:starIcons()}`.

### Naming conventions

- C# namespaces: `TsukiyukiMiyako.Scripts` for content, `Tsukiyuki_Miyako.MiyakoModCode` for base classes
- Card portrait images: `Tsukiyuki Miyako/images/cards/{ClassName.ToLowerInvariant()}.png`
- Big/detailed portraits: `Tsukiyuki Miyako/images/card_portraits/big/{id_lower}.png`
- Power icons: `Tsukiyuki Miyako/images/powers/{id_lower}.png` with fallback `power.png`
- Relic icons: `Tsukiyuki Miyako/images/relics/{id_lower}.png` with fallback `relic.png`
- Godot scene paths: `res://Tsukiyuki Miyako/scenes/{type}/{name}.tscn`

### Dependencies

- **BaseLib** — community modding library (referenced from `Sts2Dir/mods/BaseLib/BaseLib.dll`)
- **0Harmony** — runtime patching (referenced from the game's data directory)
- **sts2.dll** — the game's own assembly (referenced from the game's data directory)
