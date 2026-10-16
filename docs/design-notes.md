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

## 1.6 流血 DoT —— 已实装为可复用机制（供攻击牌附带）

**状态**：`Scripts/Powers/BleedPower.cs` 已实装并编译通过；配套示例卡（破片飞弹）已按要求删除。**待接入到攻击牌上作为附带 DoT。**

**机制（与中毒的区别）**
| | 中毒 `PoisonPower`（原版） | 流血 `BleedPower`（本 Mod） |
|---|---|---|
| 伤害 | 每回合 = 当前层数（递减） | **固定 4 点** |
| 层数含义 | 伤害值 | **剩余回合数** |
| 叠加 | 加伤害 | **加持续回合** |
| 结算时机 | 目标回合开始（`AfterSideTurnStart` + `participants.Contains(Owner)`） | 同左 |
| 是否穿格挡 | 是（`Unblockable \| Unpowered`） | 是 |

**接入方式（写在攻击牌 `OnPlay` 里）**
```csharp
// 先结算伤害，再挂流血
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCardCompatibility(this, cardPlay)
    .Targeting(cardPlay.Target!)
    .Execute(choiceContext);

await PowerCmd.Apply<BleedPower>(
    choiceContext,
    cardPlay.Target!,
    DynamicVars["BleedPower"].BaseValue,   // 层数 = 持续回合数
    Owner.Creature,
    this);
```
- 变量声明：`new PowerVar<BleedPower>(4m)`（示例值）
- `ExtraHoverTips` 加 `HoverTipFactory.FromPower<BleedPower>()`，让玩家能悬停看说明
- 玩家侧也能挂（判定是 `participants.Contains(Owner)`，不分敌我），可做"自己也吃流血"的透支玩法

**平衡注意**
- 总伤害 = `4 × 层数`，但**目标提前死亡就吃不满** → 天然"打高血量目标强、清小怪亏"
- 1 费档建议层数 ≤ 4（总上限 16），否则容易超过同费爆发牌
- 固定伤害不吃力量/易伤加成（`ValueProp.Unpowered`）；要让它吃加成，去掉 `Unpowered`

**遗留**
- [ ] 图标 `images/powers/tsukiyukimiyako-bleed_power.png`（暂无，走 `power.png` 兜底，不会崩但不好看）
- [ ] 若要做"流血不递减 / 加速结算"类支撑牌，需在 `BleedPower` 上加钩子（参考 `AccelerantPower` 对中毒的做法）

## 1.8 寂寞博弈：「跟你爆了」（已定方向）

**设计定性**：把「寂寞」从纯惩罚状态，变成**双向开关**。
- 羁绊高 = 格挡厚（每层 +1 格挡 = 每层 1 点敏捷）+ 无人机被动高 = 安全
- 羁绊掉到 0 = 寂寞 = 变弱，**但此时解锁一两张"拿命换结果"的牌**

这样玩家要做一个真正的决策：**要不要停在低羁绊、顶着寂寞去打一发**。同时它和"不能虐待她"的保护设计不冲突——她不是怨恨你，是「我陪你一起」。

**核心机制：代价与收益都咬在生命上**

1. **门槛（硬性）**：只有寂寞状态下才能打出
```csharp
// 只有寂寞状态下才能打出（Fatigue.cs 里已有 ShouldPlay 的现成范式）
public override bool ShouldPlay(CardModel card, AutoPlayType _)
    => Owner.Creature.GetPowerAmount<LonelinessPower>() > 0;
```
再配 `ShouldGlowRedInternal` 做红字提示。

2. **伤害倍率 = 已损生命值**（这是关键，越惨越强）
原版 `Unleash` 提供了标准写法（`CalculatedDamageVar` + `WithMultiplier` 拉委托）：
```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => [
    new CalculationBaseVar(0m),
    new ExtraDamageVar(0.5m),                     // 每 1 点已损生命 → 0.5 伤害
    new CalculatedDamageVar(ValueProp.Move)
        .WithMultiplier((card, _) =>
            (card.Owner.Creature.MaxHp - card.Owner.Creature.CurrentHp) * 0.5m)
];
```
> `Creature.MaxHp` / `Creature.CurrentHp` 都是公开属性，直接可用。

3. **代价：失去生命**，必须用 `ValueProp.cardHpLoss`（= `Unblockable | Unpowered | Move`），**否则会被格挡和减伤影响，就不是"沉重代价"了**。

4. **安全阀**：当前生命 ≤ 代价时不可打出（或代价后至少保留 1 点），避免直接自杀。原版自伤牌也是这个处理。

**两张草案**

| 牌 | 费 | 类型/稀有 | 效果 | 代价 |
|---|---|---|---|---|
| A.「和你一起」 | 3 | 攻击 / 稀有 | 造成 `已损生命 × 0.5` 伤害 | 失去 50% **当前**生命 |
| B.「兔子的獠牙」 | 0 | 技能 / 罕见 | 获得 2 能量、抽 2 张 | 失去 10% **最大**生命 |

**需要拍的设计问题**
- [ ] 是否允许"故意不攒羁绊"来长期保持寂寞状态？
  - 允许 → 可发展成一条**自伤爆发流派**（高风险高回报），但要注意别强过正常玩法
  - 不允许 → 需要给寂寞加"每回合 -1 层"或"N 回合后消失"，让它只能是短暂窗口
  - 倾向：**允许**（给了角色第二种玩法，且"寂寞"本就适合当状态而非即时惩罚）

**实现要点备忘**
- `ShouldPlay(CardModel, AutoPlayType)` 在原版 `Fatigue.cs` 里有现成 override 范式
- 参考实现：`Spite`（按"本回合是否失去过生命"做条件）、`Unleash`（按数值算伤害）
- 羁绊侧：`LonelinessPower` 层数 = `3 + |羁绊|`，重新获得羁绊即清除（保护设计，不动）

## 1.9 配备牌→Token 化（已定方向）

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

## 2. 羁绊体系诊断（已定结论）

**设计前提（作者定）**：羁绊 = **越多越好**的正向资源，不能"虐待她"（掉到 0 挂寂寞就是保护伞）。

**收益侧：没问题，不用动。**
- 每层羁绊 = **每张格挡牌 +1 格挡**（本质上"每层送 1 点敏捷"）+ 无人机被动伤害 +1
- 收益**线性、无边际递减、无封顶**，且会吃到卡组里所有格挡牌（乘法式放大）

| 格挡牌 | 基础 | 羁绊5 | 羁绊10 |
|---|---|---|---|
| 防御 | 5 | 10 | 15 |
| 备战 / 绳降 | 8 | 13 | 18 |
| 小队整备 | 9 | 14 | 19 |
| 小憩 | 13 | 18 | 23 |

**结构缺口：只有一条 —— 缺"战斗内可持续增长"的来源。**
- 现有 8 个来源**全部一次性**：遗物开局 1、主张所有权 2、信任之契 5、老师的嘱托 1、偷跑开始 1、小队整备 1、安心之触 1、星空奇迹（每能力牌 1）
- 一局可拿约 13~15 层且不可再生 → 前 2~3 回合定型，之后是静态增益
- 连带后果：`夜景相伴`（获羁绊抽牌）、`温柔依赖症`（获羁绊给能量）在第一张之后即成废牌

**要做**
- [ ] **一张可重复增长的羁绊引擎（能力牌）**：优先"每回合开始 +1 层"；备选"每打出攻击牌 +1 层（每回合上限 1~2）"
- [ ] 上限控制：**一回合 +1 层就够**（1 层 = 1 敏捷，已经很强），不要给 2

**不要做**
- ❌ 任何"消耗羁绊换收益"的方向（与"越多越好"冲突）；`光明正大的偷看` / `浪间牵念` 这两张消耗牌建议后续改成别的代价（扣血、弃牌等），把羁绊变成只增不减的资源
- ❌ 不要再给羁绊加"每层收益坡度"（收益本来就够）

## 2.5 其他未做方向（备忘）

- **结构缺口**：4 费完全没有卡；普通稀有度没有一张全体攻击牌。
- **配备牺牲线**：消耗若干张配备牌换能量/抽牌（配合 `军备箱`、`最可靠的队长` 形成生成→燃烧→回收循环）。
- **弹夹闭环**：目前产多耗少（8 产 8 耗），缺一张"抽干全部弹夹"的兑现牌。

## 3. 已知问题（待修）

**文本/数值不一致（已逐条用"key 前缀 + 代码对照"复核）**
- `RABBIT31_SMG_POWER`：文本写 "+1"，但代码 `PowerVar<Rabbit31SmgPower>(2m)` → **文本应改为 +2**（卡 `RABBIT31_SMG` 写 +2 是对的）
- `JUSTICE_FOR_YOU_POWER`：文本写"大于5点"，但代码 `if (senseiCount < 5) return;` → **实际是 ≥5，文本应改为"达到5点"**（卡面文本是对的）
- `SUPPORT_SAKI` 措辞误导：卡面写"本回合**失去**力量"，实际 `SakiPower` 是**临时削减、敌方回合结束回还**。建议改成"本回合内力量-3（临时）"
- `OPEN_PEEK` 用词：卡面写"失去"羁绊，实际是"消耗"（与 `WAVES_LONGING` 的"消耗"不一致）
- 个别描述缺结尾句号（`C4`、`SRT_JUSTICE` 等）

**已在实测中确认的行为（不要再误判）**
- 羁绊**不能为负**，但**能触发寂寞**：羁绊掉到 0 时 `AfterPowerAmountChanged` 先于移除执行，寂寞照挂
- 实测羁绊可为负、且寂寞可叠到力量/敏捷 **-4**（与 `3 + |羁绊|` 的关系待查；见 `SenseiPower` 里的临时 `Log.Info` 调试日志）
- `FATIGUE` 的"5 张"与代码一致（`MaxCardsPerTurn = 5`），**不是 bug**

**待评估**
- `ARMOR_PIERCE_DAMAGE` 数值偏低：1 费单体仅 3 伤（单体基准 6），建议提到 5~6 伤或改成 0 费纯功能牌
