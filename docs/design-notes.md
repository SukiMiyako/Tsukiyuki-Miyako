# 宫子 Mod — 卡牌设计设想（未实装）

> 记录灵感和待做方向。**已实装**的内容不在这里，以 `Scripts/Cards/*.cs` 为准。

## 1. 溅射 / 相邻伤害（来源：配备 RPG 的灵感）

**动机**
- 牌池缺"核爆感"：单段高伤只有 `高空突袭 21/3费`、`引爆 25/2费(衍生)`。
- 多段伤害被**蜂群术士（Entomancer）**惩罚：`Personal Hive x` = 每次被攻击命中就往抽牌堆塞 `x` 张 `Dazed`。
- 基础游戏没有相邻机制：`TargetType` 只有 `None/Self/AnyEnemy/AllEnemies/RandomEnemy/AnyPlayer/AnyAlly/AllAllies/TargetedNoCreature/Osty`，`AttackCommand` 只能"一组目标一个伤害值"。

**技术可行性（已确认，不需要 Harmony 钩子）**
- `CreatureCmd.Damage(ctx, IEnumerable<Creature> targets, decimal amount, ValueProp, dealer, cardSource)` 可接收任意目标列表 → 分两次结算即可实现"主目标高伤 + 相邻低伤"。
- 相邻判定：`CombatState.Enemies` 的顺序即站位；需要严格左右时先 `SortEnemiesBySlotName()`。
- 单怪战斗要跳过溅射，否则主目标会吃到两份伤害。
- 注意：打相邻目标 = 多一次命中，在养蜂人面前会各自多塞 `Dazed`（天然的平衡代价）。

**待选变体设想**
| 方向 | 想法 |
|---|---|
| 基础 RPG | 3费 / 主目标 14 单段 + 相邻 5。升级：主 18 / 相邻 7 |
| 直击未命中的溅射 | 主目标未造成伤害或未击杀时，相邻伤害翻倍 |
| 冲锋弹 | 3费高伤，但只能打最左/最右的敌人（站位条件） |
| 集束弹药 | 主目标满伤；相邻只受一半，但**必定命中**（无视格挡） |
| 穿透 | 主目标 + 直线上的敌人，每穿过一个目标伤害衰减 |

**已定的设计原则**
- 主目标必须是**单段**（规避多段惩罚，这才是这张牌的意义）。
- 溅射数值要压住：不能高到把单段变成变相多段 AoE。
- 主题属于「配备」（消耗、可被军备箱生成、与 RPG 意象契合）。

## 1.2 数值评估基准（原版实测，写新牌前先看这里）

> **血泪教训：不要只看伤害定强弱。** 本 Mod 的牌是"效果为主、伤害为辅"（咲 的减力量、破门 的抽牌都是主要价值），按纯伤害比对会全部判错。

**伤害基准（原版 28 张群攻攻击牌实测）**

| 档位 | 单体 | 群攻 |
|---|---|---|
| 1 费 | 6（打击） | **8**（匕首雨 4×2、Breakthrough 9、Sow 8、CrushUnder 7） |
| 2 费 | 10~13 | **12~16**（HowlFromBeyond 16、Stomp 12；Hyperbeam 28 为稀有） |
| 3 费 | 16~21 | 16+ |
| 稀有加成 | 前置条件可到 8~10/费 | 9~10/费 |

- **多段 vs 单段**：多段会被蜂群术士（Entomancer）的 `Personal Hive` 按命中次数惩罚，所以"单段同数值"应视为**更强**，同比该略降数值。

**效果定价（折成能量）**

| 效果 | 约等于 | 依据 |
|---|---|---|
| 抽 1 张 | **1 费** | Backflip 1费抽2、PommelStrike 1费抽1 |
| 格挡 5 | ≈ 1 费 | 防御 |
| 虚弱 1（全体） | ≈ 1 费 | 敌方攻击伤害 **×0.75**（`WeakPower`） |
| 易伤 1（全体） | ≈ 1 费 | 受到伤害 **×1.5**（`VulnerablePower`） |
| 力量 -3（全体，临时） | ≈ 1 费 | 每个攻击者每回合 -3 伤（`SakiPower`） |
| 弹夹 / 星 | 1 费 ≈ 2 个 | 应急换弹 1费2弹夹 |

**评估流程**：先把非伤害效果折成能量，再看剩余能量对应的伤害是否达标。

### 按此基准复核过的牌（结论）
| 牌 | 结论 |
|---|---|
| 高爆手雷 1费全体8 | **达标**（1费群攻标准值就是 8） |
| 破门而入 2费全体10+抽1 | **达标偏上**（10群伤 + 抽1≈1费） |
| 支援咲 2费全体10 + 力量-3 | **强生存牌**（减伤收益 ≈1费，且是多段友好） |
| 压制射击 2费全体12 + 虚弱1 | **达标偏上** |
| 穿甲弹 1费单体3 | 真偏低（剥格挡/人工制品的附加分不够补 3 点伤害缺口） |

## 1.5 标枪飞弹 —— 规划（已定稿，待实装）

**卡面设定**
- 名称：标枪飞弹
- 类型：**配备** / 技能牌 / 单体目标（`TargetType.AnyEnemy`）
- 费用：1 费
- 稀有度：普通（可再议）
- 关键词：`Equipment` + `CardKeyword.Exhaust`（配备牌体系默认消耗）
- 效果：**锁定一名目标**，需要 1 回合引导；**下回合开始时**对其造成伤害
- 数值：基础 **17 伤**；升级 → **21 伤**（这是"收尾单段牌"方向里的 1 费档）
- 设计定位：单段延迟爆发，规避多段被惩罚的问题；延迟本身是折扣，所以基础伤害比 `精准突袭(15)` 高一档

**技术方案（已验证可行，无需 Harmony 钩子）**
- 卡片本体：`OnPlay` 里对选定目标施加 `JavelinMissilePower`，并把伤害值传入 power
- power：`PowerInstanceType.Instanced`（每张牌一个实例），挂在**目标敌人**身上（`Owner` = 被锁定的敌人）
  - `SetDamage(decimal)` 把卡牌 `DynamicVars.Damage.BaseValue` 存进 power（照 `TheBombPower` 的写法）
  - 重写 `AfterPlayerTurnStart(PlayerChoiceContext, Player)` → 对 `Owner` 结算单段伤害（`ValueProp.Move` + `cardSource: this`，确保吃的加成与卡牌伤害一致）→ `PowerCmd.Remove(this)`
- 目标死亡/离场时 power 随之移除，无需额外清理逻辑
- 数据流：卡牌数值 → `SetDamage` → power 回合钩子 → 单段结算 → 移除

**待办**
- [ ] `Scripts/Cards/JavelinMissile.cs`
- [ ] `Scripts/Powers/JavelinMissilePower.cs`
- [ ] `cards.json` 加 `TSUKIYUKIMIYAKO-JAVELIN_MISSILE.title/description`
- [ ] 卡图 `images/cards/tsukiyukimiyako-javelin_missile.png`
- [ ] power 图标 `images/powers/tsukiyukimiyako-javelin_missile_power.png`

## 1.8 配备牌→Token 化（已定方向）

**问题**：消耗型配备牌出现在奖励/商店里，玩家"拿也不是不拿也不是"——它们的定价本来就是一次性消耗品的价（比非消耗便宜），却按永久卡组成员在卖。

**决定**：按"能不能重复用"切分，而不是按"是不是配备"切分。

**① 进 Token（9 张，一次性使用型）**
高爆手雷、空尖弹、阔剑地雷、易碎弹、闪光弹、催泪瓦斯、穿甲弹、C4、震撼弹

**② 留在池子（4 张）**
| 牌 | 保留理由 |
|---|---|
| 军备箱 | **生成器**（它本身是配备），进 token 就没人能生成配备了 |
| 激光镭指 | 增幅类，玩家应能主动抽到并围绕它构筑（且数值回报大） |
| 双孔制退器 | 增幅类，同上 |
| 弹鼓 | 资源类，SMG 体系的起手件 |

**技术结论（已确认）**
- `CardRarity.Token` 确实不会被奖励/商店抽到：`GetNextAllowedRarity` 白名单里 `Token → None`（`GetNextHighestRarityWithWrapping` 也不含 Token）
- **但军备箱生成照常工作**：`CardFactory.FilterForCombat` 只排除 `Basic / Ancient / Event`，Token 放行
- Token 化后**依然在图鉴可见**（按 `ShouldShowInCardLibrary` 过滤，建议保持 `true`）
- 副作用：失去稀有度边框；`GetDefaultTransformationOptions` 会把 Token 牌当无色处理（变化类效果再也变不出配备，可接受）

**必须补的连带项**
- [ ] 军备箱的生成池会缩到很小（`FilterForCombat` 放行 token，但可抽牌变少）→ **补 2~3 张新投掷物类配备喂生成池**（燃烧弹 / 绊索雷 / 信号弹之类）
- [ ] 奖励池里**"群攻"职能会空掉**（手雷、破门、压制、催泪瓦斯、闪光弹几乎全是配备型）→ **至少补 1 张达标的 1 费群攻（8~9 伤）**
- [ ] 补的新 Common 建议**不要带 Equipment 关键词**，去补别的轴（SMG / 弹夹 / 羁绊），否则等于把这轮清理又填回去

## 2. 其他未做方向（备忘）

- **师生羁绊兑换**：消耗固定层数换固定收益（贴现值：每层换 1~4）；`SenseiPower` 同时是格挡加值与充能球 Focus，目前只当计数用。
- **羁绊打穿成负数 → 寂寞**：`SenseiPower.AfterPowerAmountChanged` 里已有完整惩罚（`LonelinessPower` 层数 = `3 + |羁绊|`），可做成"透支型核爆"。
- **结构缺口**：4 费完全没有卡；普通稀有度没有一张全体攻击牌。
- **配备牺牲线**：消耗若干张配备牌换能量/抽牌（配合 `军备箱`、`最可靠的队长` 形成生成→燃烧→回收循环）。
- **弹夹闭环**：目前产多耗少（8 产 8 耗），缺一张"抽干全部弹夹"的兑现牌。

## 3. 已知文本/数值不一致（待修，不影响玩）

- `RABBIT31_SMG` 卡面写 +2，`RABBIT31_SMG_POWER` 写 +1（代码是 2）。
- `JUSTICE_FOR_YOU` 卡面写"达到5点"，power 文本写"大于5点"（代码是 `>5`）。
- `FATIGUE` 卡面写"不可打超过5张"，代码基数是 8。
- `OPEN_PEEK` 卡面写"失去"，实际是"消耗"羁绊（`WAVES_LONGING` 用的是"消耗"）。
- **`SUPPORT_SAKI` 措辞误导**：卡面写"所有敌人在本回合**失去**力量"，但 `SakiPower` 是**临时削减、敌方回合结束就还回去**。建议改成"本回合内[gold]力量[/gold]-3（临时）"，否则会严重误判强度（开发时就误判过一次）。
- **`ARMOR_PIERCE_DAMAGE` 数值偏低**：1 费单体仅 3 伤，剥格挡/人工制品的附加分补不上缺口（单体基准 6）。建议提到 5~6 伤，或明确改成"纯功能性"定价（0 费）。
- 个别描述缺结尾句号（`C4`、`SRT_JUSTICE` 等）。
