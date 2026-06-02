# Tsukiyuki Miyako —— 杀戮尖塔2 角色 Mod

> 「SRT的正义，永远不会动摇。」

为《杀戮尖塔2》(Slay the Spire 2) 添加来自《蔚蓝档案》(Blue Archive) 的自定义角色——**月雪宫子**。

## 角色介绍

月雪宫子是 SRT 特殊学园 RABBIT 小队的队长。沉默而温柔，始终安静地守护着同伴。她以"弹夹"作为第二资源，围绕**师生羁绊**、**配备/SMG/小队支援**等关键词构筑卡组。

## 机制特色

### 弹夹（Stars）

宫子的第二资源系统。部分卡牌（特别是 SMG 类卡牌）需要消耗弹夹才能打出，需要合理管理弹夹资源。

### 师生羁绊（Sensei Bond）

核心成长机制。每层师生羁绊使从卡牌中获得的格挡 +1，同时提高无人机被动伤害。通过卡牌和能力可持续叠加。

### 闪光无人机（Flash Scout Drone）

专属充能球。被动：回合结束时对随机敌人造成伤害并获得格挡；激发：造成高额伤害。激发伤害随回合增长。

### 关键词

| 关键词 | 效果 |
|--------|------|
| **配备** | 若由军备箱生成的，则免费打出 |
| **小队支援** | 若由「RABBIT小队，集结!」获得的，则免费打出 |
| **SMG** | 可被专武（RABBIT-31 式冲锋枪）增幅伤害 |

## 卡牌一览

> 共计 75+ 张卡牌，涵盖攻击、技能、能力三种类型。

### 初始卡牌
打击、防御、冲锋扫射、应急换弹

### 配备（Equipment）
高爆手雷、闪光弹、催泪瓦斯、震撼弹、C4、阔剑地雷、钨芯穿甲弹、空尖弹、易碎弹……

### SMG
冲锋扫射、短点射、抵近射击、火力倾泻、狂野射击、压制射击……

### 小队支援（Squad Support）
小队支援：咲 / 萌 / 美游、RABBIT小队，集结!

### 能力（Powers）
长线作战、能量凝胶、省着点用、星空奇迹、雨中的坚守、无人机同频、RABBIT-31 式冲锋枪、严阵以待、激光镭指、温柔依赖症、眼里闪着光的兔子、We are Rabbits!、碎在雨中的信念、因你而存在的正义……

## 专属遗物

**破旧的名牌**（初始遗物）：每回合开始时抽牌，战斗开始时获得弹夹和师生羁绊。

> "这是宫子在训练生时代使用的破旧名牌。表面上看似无足轻重的布片，但对宫子而言，却是承载着过去梦想的珍贵之物。"

## 安装

### 前置要求

- [Slay the Spire 2](https://store.steampowered.com/app/2864780/Slay_the_Spire_2/)
- [BaseLib](https://github.com/Alchyr/BaseLib-StS2) —— 社区 Mod 框架
- [sts2_custom_character_sprite_kit_bdd](https://wwarj.lanzout.com/i7lYE3l5w4hi)

### 步骤

1. 安装 BaseLib：下载后解压到 `<STS2安装目录>/mods/` 下
2. 下载本仓库的 Release 压缩包，解压到 `<STS2安装目录>/mods/` 下
3. 启动游戏，在 Mod 选择界面启用「Tsukiyuki Miyako」

### 文件夹结构

```
mods/
└── Tsukiyuki Miyako/
    ├── Tsukiyuki Miyako.dll
    ├── Tsukiyuki Miyako.json
    └── Tsukiyuki Miyako.pck
```

## 参考

- [【杀戮尖塔2】自定义角色Mod制作教程](https://tutorials.sts2modding.com/)
- [BaseLib 源码 & 文档](https://github.com/Alchyr/BaseLib-StS2)

## 开发

### 环境

- Godot 4.5.1 + .NET 9.0
- 修改 `Tsukiyuki Miyako.csproj` 中的 `<Sts2Dir>` 指向你的 STS2 安装目录

### 构建

```bash
dotnet build
```

构建完成后会自动将 DLL 和 JSON 复制到 `<Sts2Dir>/mods/Tsukiyuki Miyako/`。

### 项目结构

```
├── Scripts/                    # 卡牌、能力、充能球实现
│   ├── Cards/                  # 75+ 张卡牌
│   ├── Powers/                 # 25+ 个能力
│   ├── Orbs/                   # 闪光无人机充能球
│   ├── Entry.cs                # Mod 入口
│   └── MyKeywords.cs           # 自定义关键词注册
├── MiyakoModCode/              # 基础抽象类 & 角色定义
│   ├── Character/              # 角色、卡池、遗物池、药水池
│   ├── Cards/                  # 卡牌基类
│   ├── Powers/                 # 能力基类
│   ├── Relics/                 # 遗物基类
│   └── Nodes/                  # 自定义 Godot 节点
└── Tsukiyuki Miyako/           # Godot 场景 & 美术资源
    ├── scenes/                 # 角色动画、休憩点、商店场景
    ├── images/                 # 卡图、能力图标、遗物图标
    └── localization/zhs/       # 简体中文本地化
```

## 致谢

- 角色「月雪宫子」出自《蔚蓝档案》(Blue Archive) © NEXON Games & Yostar
- Mod 框架：[BaseLib](https://github.com/Alchyr/BaseLib-StS2)
- 《杀戮尖塔2》© Mega Crit Games

## 免责声明

本 Mod 为玩家自发创作的免费同人作品，与 NEXON Games、Yostar 及 Mega Crit Games 无关。

- 本 Mod 中的角色形象与设定归原版权方所有
- 本 Mod 仅供学习与交流，不得用于商业用途
- 若存在侵权问题，请联系作者进行处理

## License

MIT
