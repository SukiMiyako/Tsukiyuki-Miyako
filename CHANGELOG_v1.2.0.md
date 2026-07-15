# Tsukiyuki Miyako v1.2.0 更新日志

## 🆕 新增内容

### 新卡牌
- **信任之契（Oath of Trust）** — 0费 Ancient·技能牌。获得5层师生羁绊。消耗。
- **冲锋爆发（SMG Burst）** — 1费 Uncommon·攻击牌。造成6点伤害2次。

### UI 修复
- **能量表盘文字描边** — 修复能量计数器文字在游戏中无描边的问题。通过自定义 `MiyakoEnergyLabel` 节点使用 `_Draw()` 手动渲染描边，不再依赖引擎 MSDF 字体支持。

## 🐛 Bug 修复

- **TeamPrepare「小队整备」CanonicalModelException** — 移除了 Uncommon 卡牌上错误挂载的 `CardTag.Defend` 标签，该标签仅适用于 Basic 牌，导致模型校验异常。
- **JusticeForYou「因你而存在的正义」触发条件** — 修复条件判断逻辑：`senseiCount <= 8` → `senseiCount < 5`，与卡牌描述"师生羁绊达到5点"保持一致。同步更新中文本地化文本。
- **ShiningRabbitPower 描述格式** — 修正中文本地化中 `{Amount:diff()}` → `{Amount}`，避免错误的差值格式。

## 🧹 代码清理

- 移除 `RabbitSquadAssemble.cs`、`SupportMiyu.cs`、`SupportMoe.cs`、`SupportSaki.cs` 中重复的 `using MegaCrit.Sts2.Core.Commands;`
- 删除未使用的 `saki_energy_counter.tscn` 场景文件
