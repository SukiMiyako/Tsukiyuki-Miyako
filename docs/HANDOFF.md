# 交接文档（2026-10-17 会话）

> 给下一个会话/助手的上下文。**先读 `CLAUDE.md` 的工作原则，再读这份。**
> **最新一轮（2026-10-18）的变化看 §7，上面 §0~§6 是上一轮的结论。**

---

## 0. 一句话现状

宫子 mod 上一轮：新增 8 张牌 + 5 个 power，重做了「兔子的獠牙」「星空奇迹」「因你而存在的正义」，
修了能力提示框的变量解析。**已 push 到 `origin/main`，提交 `81178b1`。**

本轮（§7）：11 张卡图全部补齐、新增 6 张牌 + 1 个 power、新增 **RABBIT 关键词**、
**所有配备牌 Token 化**、獠牙加 +3 能量。

---

## 1. 铁律（来自 CLAUDE.md + 用户明确要求）

- **构建与导出归人类**（`dotnet build` / Godot 导出都不要自己跑，包括"只想验编译"）。
  想确认 API/签名 → 读反编译源码 `D:\slay\_miyako_decomp\sts2src`。
- **不要碰另外两份 Miyako**（开发版 + Workshop 旧版）。
- **不要擅自改存量牌**、不要自己合成音频、不要在没有指示时"顺手优化"。
- **不要替用户做设计决定**：给方案 + 建议，等拍板；用户常在实施后自己调数值。
- **角色外观一律以参考图为准**，提示词里不要写死发色/瞳色/发型（我编错过一次黑发，被骂）。
- 回复要短、直接落码；长篇设计分析只在被问到时给。
- `MSB3021`（拷贝 DLL 到 mods 失败）是正常的，游戏占着文件。

---

## 2. 项目结构速查

| 东西 | 位置 / 规则 |
|---|---|
| 卡牌 | `Scripts/Cards/*.cs`，`[Pool(typeof(MiyakoCardPool))]` + `CustomCardModel` |
| 能力 | `Scripts/Powers/*.cs`，`MiyakoModPower`（图标缺失自动回落 `power.png`） |
| 本地化 | `Tsukiyuki Miyako/localization/zhs/{cards,powers,card_keywords,characters}.json` |
| 本地化 key | `TSUKIYUKIMIYAKO-<类名 SCREAMING_SNAKE>.title / .description` |
| 卡图 | `images/cards/tsukiyukimiyako-<key小写>.png`（4:3，老图标准 2364×1773） |
| power 图标 | `images/powers/tsukiyukimiyako-<key小写>.png`（64×64）与 `powers/big/...`（256×256） |
| 音频 | `Audio/*.ogg / *.mp3`；`SfxCmd.Play("res://Audio/文件名")`；**新文件必须在 Godot 里导入一次生成 `.import`**，否则导出后静默无声 |
| 设计文档 | `docs/design-notes.md`：**§1.2 数值基准（写新牌前必看）**、§1.9 配备→token 计划、§2 羁绊体系诊断 |
| 角色资料 | `docs/MIYAKO_LORE_REFERENCE.md` |

---

## 3. 本轮新增 / 改动

### 新卡（8 张）

| 卡 | 费用 | 类型/稀有 | 效果 |
|---|---|---|---|
| 标枪飞弹 `JavelinMissile` | 1费+2星 | 技能/普通/配备+消耗 | 锁定目标，下回合开始 0.75s 延迟后单段伤害（锁定音 / 发射音） |
| 扫射 `Strafe` | 1费+2星 | 攻击/普通/SMG | 对全体 3×3；升级 4×3 |
| 掠夺射击 `PlunderShot` | 1费+1星 | 攻击/罕见/SMG | 12 伤；击杀则给 1 张配备（升级 +3） |
| 看准再打 `CarefulAim` | 1费+1星 | 攻击/罕见/SMG+消耗 | 10 + 目标已损失生命×50%，**封顶 40**；升级上限 40→50 |
| 帮大忙了！老师！`ThanksSensei` | 0费 | 技能/普通 | 失去 1 层羁绊 → 1 张随机配备（可走负数进寂寞） |
| 这可比枪好使多了 `BetterThanAGun` | 0费+3星 | 技能/普通/消耗 | 2 张随机配备；升级 3 张 |
| 我……我没事 `ImFine` | 1费 | 技能/罕见/消耗 | **仅寂寞**：失 2 血，+3 力量/+3 敏捷；寂寞结束一并消失 |
| 该反击了 `FightBack` | 2费 | 技能/普通 | 16 格挡；到你的下回合开始未掉血 → 全体 20 伤；升级 20→25 |

### 新 power（5 个）

| power | 作用 |
|---|---|
| `NotOneStepBackPower`「一步不退」 | 死亡层防死一次：`ShouldDie=false` → 降到 1 血 → 播 `broke.mp3`(碎盾) → 自移除 |
| `RabbitFangPower`「兔子的獠牙」 | 獠牙状态化：寂寞结束收回 +力量/−敏捷，**脆弱保留** |
| `ImFinePower`「还撑得住」 | 同上套路，寂寞结束收回 +力量/+敏捷 |
| `FightBackPower`「蓄势反击」 | 记录本轮是否掉血（`AfterCurrentHpChanged`），下回合开始结算反击 |
| `JavelinMissilePower`「标枪锁定」 | Instanced；`Amount` = 伤害；下回合开始发射 |

### 改动

- **星空奇迹**：从"每打出能力牌+1羁绊"（会与「温柔依赖症」「与你共赏夜景」形成无限循环）改成
  **"从卡牌获得的羁绊 +1"**（`ModifyPowerAmountGivenAdditive`），并去掉 `cardSource == null` 的过滤
  （偷跑开始/小队整备/OrigDream 都传 null，之前吃不到）。
- **因你而存在的正义**：0 费 → **2 费**，升级改为 −1 费（原来给固有）。
- **只可在寂寞下打出的牌统一加消耗**（兔子的獠牙 / 我……我没事）。
- **羁绊数量描述全部走 `{SenseiPower:diff()}`**；「安抚的触碰」补上缺失的变量并改读变量。
- 卡名对调：无实体牌 = **小伤而已**（`JustAScratch`），防死牌 = **我还不能倒下**（`ICantFallHere`）。
- 小队支援：萌 改 **4×7**，升级每段 +2；战地医疗处理订正费用与注释。
- 只可在寂寞下打出的牌、以及"花弹夹的牌统一 SMG"两条规则已落地。

---

## 4. 引擎机制备忘（本轮扒出来的，别重新扒）

### 4.1 死亡层钩子（防死）
`CreatureCmd.KillWithoutCheckingWinCondition`（`CreatureCmd.cs:489`）：
1. 血清到 0 → `Hook.BeforeDeath` → `Hook.ShouldDie`（先问一轮 `ShouldDie`、再问一轮 `ShouldDieLate`，
   **谁先返回 false 谁独占 preventer 并立刻 return**）
2. 有 preventer → `Hook.AfterPreventingDeath(preventer, creature)`
3. 紧接着 `if (creature.IsDead) Kill(recursion+1)`，`recursion >= 10` 抛异常
   → **`AfterPreventingDeath` 里必须把血拉回 >0**
4. `Kill(_, force: true)` 绕过一切防死；`MaxHp <= 0` 时引擎在 ShouldDie 之前就判死

参考实现：`LizardTail`（遗物）、`FairyInABottle`（药水）、`MockPreventDeathPower`。

### 4.2 掉血层 vs 伤害上限
- `ModifyHpLostBeforeOsty / AfterOsty(+Late)`：`BufferPower`（归 0 并消耗）、`IntangiblePower`（压到 1）
- `ModifyDamageCap`：`HardToKillPower`、`IntangiblePower`（每段封顶）
- 多段攻击 = `AttackCommand` 每段各调一次 `CreatureCmd.Damage`（`AttackCommand.cs:653`），**每段独立判死**
  → 一次性防死在"致死段之后还有剩余段"时会穿透

### 4.3 固有（Innate）
`CombatManager.cs:654-671`：起手抽牌数 = `max(5, 固有牌数)`，上限 `CardPile.MaxCardsInHand = 10`；
固有牌全部被顶到牌堆顶。→ **固有 ≥5 张时起手 100% 是固有牌**，容易"第一回合大出血"。

### 4.4 ★ 能力提示框只能认 `{Amount}`
`PowerModel.HoverTips`（`PowerModel.cs:350-398`）：
- 只有「有 `.smartDescription` key」且「战斗中 mutable 实例」时才 `DynamicVars.AddTo(...)`
- 其余情况只注入 `{Amount}` / `{singleStarIcon}` / `{energyPrefix}`
- 卡牌上的 `HoverTipFactory.FromPower<T>()` → `GetDumbHoverTip()`，**永远解析不了自定义变量**

→ **power 的数值请存进 `Amount`，描述用 `{Amount}`。**（本轮据此重写了 3 个 power）

### 4.5 星费 / SMG
- 星费（弹夹）：`public override int CanonicalStarCost => N;`；X 费用 `HasStarCostX`
- 项目规则：**花弹夹的牌统一带 `MyKeywords.Smg`**（例外：「轻装上阵」用户明确说"另外的"）
- `RABBIT-31` 的 +2 伤害是用 Harmony 补丁 `Scripts/Patches/Rabbit31SmgPowerDamagePatch.cs` 实现的
  （`Rabbit31SmgPower` 本身是空的，别以为是 bug）

### 4.6 羁绊 / 寂寞
- `SenseiPower`：`AllowNegative = true`，同时提供格挡加成（`ModifyBlockAdditive`）与无人机 Focus
- 羁绊 ≤ 0 → `SenseiPower.AfterPowerAmountChanged` 挂 `LonelinessPower`（层数 = `3 + |羁绊|`，每层 −1 力量/−1 敏捷）
- 羁绊 > 0 → 把 `LonelinessPower` 清零；**清零那一刻的 `Hook.AfterPowerAmountChanged` 就是"寂寞结束"**
  （`PowerCmd.ModifyAmount` 先 SetAmount 再派发）
- 「星空奇迹」用 `ModifyPowerAmountGivenAdditive` 做 +1（纯数值修正，不额外触发 温柔依赖症/与你共赏夜景）

### 4.7 生成"配备"牌的写法（照 `ArmoryBox.cs`）
```csharp
CardFactory.GetForCombat(owner,
    owner.Character.CardPool.GetUnlockedCards(...).Where(c => c.CanonicalKeywords.Contains(MyKeywords.Equipment)),
    数量, owner.RunState.Rng.CombatCardGeneration);
foreach (var c in cards) { if (IsUpgraded) CardCmd.Upgrade(c); c.SetToFreeThisTurn();
    await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, owner, CardPilePosition.Random); }
```

### 4.8 动态伤害（BodySlam 式）
```csharp
new CalculationBaseVar(10m), new ExtraDamageVar(0.5m),
new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) => ...)   // lambda 必须无闭包
```

### 4.9 击杀判定（Sunder 式）
```csharp
var r = await DamageCmd.Attack(x).FromCardCompatibility(this, cardPlay).Targeting(t).Execute(ctx);
if (r.Results.SelectMany(v => v).Any(d => d.WasTargetKilled)) { ... }
```

### 4.10 音频
`SfxCmd.Play("res://Audio/xxx.ogg")`，可传音量（线性倍率，可 >1）。
需要和动作同步时：`await Cmd.Wait(秒)`（Godot 定时器；玩家开"即时模式"会跳过）。
注意 `PowerCmd.Remove` 内部自带 0.2~0.4s 等待，声音要放在它**之前**。

---

## 5. 未完成 / 待决

- **缺卡面 11 张**：看准再打 / 掠夺射击 / RABBIT1，开始清扫 /（扫射已生成待落盘）/ 这可比枪好使多了 /
  该反击了 / 我……我没事 / 小伤而已 / 兔子的獠牙 / 帮大忙了！老师！ / 我还不能倒下
- **缺 power 图标 5 个**：标枪锁定 / 一步不退 / 兔子的獠牙 / 寂寞 / 小队支援：咲
- 「温柔依赖症」是否收成"每回合第一次获得羁绊才 +1 能量"（最后一个"白赚手牌"点，未做）
- 4 张传 `cardSource: null` 的羁绊牌（偷跑开始 / 小队整备 / OrigDream / OrigDreamUpgraded）要不要统一传 `this`
- §1.9 连带项：补 2~3 张**投掷物 token** 喂生成池（1 费群攻已由「扫射」补上）
- 池子结构问题（已诊断未动）：Common 偏少、无 4 费牌、Power 无 Common 档、
  无实体/闪避轴只有 2 张、池内没有治疗牌、羁绊产出 9 张 vs 消耗 3 张
- 「我还不能倒下」的升级仍是"固有"（曾讨论过改 2→1 费，未定）

---

## 6. 卡图提示词策略（当前做法）

- 用户用**对话式生图智能体**，积分有限 → **一次只出一张，不对就用"只改 X"的句子迭代**。
- **画整张**（人物 + 场景 + 全特效），由用户自己裁切取舍；不再写"纯武器特写"的受限构图。
- 参考图分工（4 张）：① 角色外观（一律照它，**不要写外观细节**）② 枪身造型（白色＋浅蓝科幻冲锋枪、
  黑色消音器）③ **弹鼓**（圆盘形，装在枪身下方）④ 战斗氛围（枪口火花 / 硝烟 / 子弹曳光）
- 负向必备：写实军用枪、木托、突击步枪、两把枪、看镜头、空洞发呆、张嘴喊话、文字水印边框、Q版/3D/写实照片风
- 眼神和动作要**写成肌肉级细节**（眯眼、眉轻皱、视线沿枪口方向、半跪探身、身体扭转、后坐带动肩膀），
  只写"冷静"会退回呆滞立绘。

---

## 7. 本轮（2026-10-18）变化

### 7.1 新增关键词：RABBIT
`Scripts/MyKeywords.cs` 加 `[CustomEnum("RABBIT")]`，文案（`card_keywords.json`）：
> 仅能在**寂寞**状态下打出；**寂寞**结束时，此牌给予的增益一并消失

**已挂**：兔子的獠牙 / 我……我没事 / 离我远点！
→ 规则写进关键词后，这三张的卡面**只留数值**（原本卡面塞不下、字体被压得很小）。

### 7.2 新增 6 张牌
| 卡 | 费 | 稀有/类型 | 效果 |
|---|---|---|---|
| 得寸进尺 `GiveAnInch` | 1费+1弹夹 | Common/攻击/SMG | 消耗1羁绊→15伤；**寂寞中反向**：获得1羁绊→12伤（先结算伤害再改羁绊） |
| 离我远点！`StayAway` | 2费 | Common/攻击 | RABBIT+消耗；15伤+1层缓冲（升级只加伤害 15→18） |
| 像你这样的大人…… `AdultsLikeYou` | 1费 | Common/攻击 | 失去1羁绊，5伤，获得5荆棘（升级荆棘5→7） |
| 与你一起 `TogetherWithYou` | 2费 | Uncommon/攻击 | 消耗全部羁绊，伤害 = 10 + 羁绊×6（升级系数 6→8） |
| 重新整备 `Reequip` | 2费 | Common/技能 | 消耗所有手牌 → 生成4张配备（免费打出）；升级减费 2→1 |
| 不能浪费 `CantWaste` | 0费 | Common/技能 | 消耗1张手牌 → +2能量（升级 +3） |

### 7.3 新增 power：`StayAwayPower`
寂寞结束时把**没被打掉的缓冲**收回（RABBIT 铁律：寂寞里的增益不带出寂寞）。
收回时用 `Math.Min(Amount, 当前缓冲)` 夹一下，避免扣成负数。套路照 `ImFinePower`。

### 7.4 所有配备牌 Token 化（14 张）
`ArmorPierceDamage / C4 / ClaymoreMine / CsGas / DualPortRecoilCompensator / FlashBang /
FragileBullet / Grenade / HollowPoint / JavelinMissile / LaserPointer / Rabbit31Drum /
RpgLauncher / StunGrenade` → `CardRarity.Token`

- 已验证：`GetUnlockedCards` 不看稀有度、`CardFactory.FilterForCombat` 只排除 Basic/Ancient/Event
  → **Token 仍能被生成器取到**
- **奖励池现在 0 张配备**，配备只能靠生成器：配备箱 / 这可比枪好使多了 / 帮大忙了！老师！ /
  掠夺射击 / 重新整备 / 严阵以待 / 最厉害的特种小队 → **生成器密度成了新命门**
- 关键词文案改成「若是由生成获得，则本回合内免费打出」，并给 `Reequip` 补上 `SetToFreeThisTurn`

### 7.5 獠牙实测定论
- 实测 **+10 力量并不超标**（失10血代价太大，差点没打过沙漏）→ 之前的「降到 +5」建议**作废**
- 真正的问题是「进了寂寞就动不了」：−5敏捷 + 寂寞自带 −1/层 = **−8 敏捷，格挡牌全废**，
  能量又已经垫在防御牌上，打不出 3 费的「RABBIT1，开始清扫」
- 已加：**打出獠牙立刻获得 3 点能量**（`EnergyVar(3)`）→ 一回合能走完 獠牙(0) + 清扫(3)
- **仍未解决**：−8 敏捷导致寂寞期防御牌归零。候选方案见 §7.7

### 7.6 卡面文本压缩原则
寂寞专属牌的规则一律走 RABBIT 关键词，卡面不再写「仅能在寂寞状态下打出」这类括注。

### 7.7 下一步候选
1. **寂寞期回费**：`LonelinessPower` 挂 `AfterEnergyReset` 每回合 +1 能量（把「纯挨打」变成「能还手」）
2. **寂寞期固定减伤**：`ModifyDamageAdditive` 对自身伤害 −3，替代已作废的格挡（不依赖敏捷）
3. 3 费斩杀凑不出费用 → 已由「不能浪费」+ 獠牙 3 能量部分解决，实测再定
4. §1.9 遗留：装备池缩小后建议补 2~3 张投掷物 token 喂生成池；Common 群攻只剩「扫射」一张

### 7.8 美术缺口
- **卡图**：6 张新卡全缺（`give_an_inch / stay_away / adults_like_you / together_with_you / reequip / pick_up_brass`）
- **power 图标**：`stay_away_power` + 上一轮那 6 个（蓄势反击/还撑得住/寂寞/一步不退/兔子的獠牙/小队支援：咲）

### 7.9 卡图提示词模板（用户已定稿，复用即可）
> 二次元卡牌插画，横构图 4:3，游戏内剧情 CG 风格。**第 1、2、3、5 张参考图是硬性标准，必须严格照画，
> 不得自由发挥、不得自行改动或美化**：人物外观照第 1 张；枪身造型（白色＋浅蓝科幻冲锋枪、黑色消音器）
> 照第 2 张；圆盘弹鼓形状与位置照第 3 张；眼睛画法照第 5 张。**只有第 4 张是参考氛围**，不照搬构图。
> 负向必备：文字/水印/边框/UI、第二个人、血迹伤口、Q版/3D/写实照片风、写实军用枪/木托、
> 看镜头、空洞发呆、枪口火花、曳光弹道、构图杂乱、主体贴边。

⚠️ 平台雷区：**「对人开火 / 敌人身影 / 残血目标 / 精准点射 / 连发火花弹壳」会被判违规**，
改写成「校准、观察、瞄准」这类动作语即可过。

---

## 8. 第二批（同日续）——寂寞体系与 SMG 双轨

### 8.1 新增 9 张牌
| 卡 | 费 | 稀有 | 效果 |
|---|---|---|---|
| 得寸进尺 `GiveAnInch` | 1费+1弹夹 | Common/攻击/SMG | 消耗1羁绊→15伤；**寂寞中反向**给回1羁绊→12伤（先伤害后改羁绊） |
| 离我远点！ `StayAway` | 2费 | Common/攻击 | RABBIT+消耗；15伤+1层缓冲 |
| 像你这样的大人…… `AdultsLikeYou` | 1费 | Common/攻击 | 失去1羁绊，5伤，5荆棘 |
| 与你一起 `TogetherWithYou` | 2费 | Uncommon/攻击 | 消耗全部羁绊，10 + 羁绊×6（升级系数→8） |
| 重新整备 `Reequip` | 2费 | Common/技能 | 消耗所有手牌 → 4张配备（免费打出）；升级减费→1 |
| 不能浪费 `CantWaste` | 0费 | Common/技能 | 消耗1张手牌 → +2能量（升级+3） |
| 请你守护着我哦 `PleaseProtectMe` | 1费 | Common/技能 | 12格挡；未失血则下回合+2羁绊（升级格挡→16） |
| 请站在我的身后 `StayBehindMe` | 1费 | Common/攻击 | 9伤 + 1羁绊（升级12伤） |
| 致命喷嚏 `DeadlySneeze` | 0费 | Common/技能 | 失2血 → 1羁绊（升级→2），梗自水宫子记忆大厅 |

### 8.2 RABBIT-31 改成双轨（段数 / 伤害）
- **多段 SMG 牌**：额外命中 1 次（`AbstractModel.ModifyAttackHitCount`，引擎现成钩子）
- **单段 SMG 牌**：伤害 +3（走 Harmony 补丁的平坦加成）
- 两条互斥：靠 `Rabbit31SmgPower._multiHitCardId`（**static** + `ModelId` 值比较）标记，
  外加「`Repeat ≥ 2` 一律按多段处理」兜底
- **踩过的坑**：实例字段 + 引用比较会静默失效 → 多段牌同时吃到额外命中和平坦伤害

### 8.3 卡面段数同步：`SmgRepeatVar`
`RepeatVar` 子类，只重写显示入口 `UpdateCardPreview` 与 `GetBaseValueForIConvertible`
（照 `CalculatedVar` 的路线），RABBIT-31 在场时卡面段数自动 +1。
**不改 `BaseValue`** → 进手牌/离手/power 移除都不用复原，每次渲染重算。
已用于：冲锋扫射/扫射/短点射/冲锋爆发/抵近射击/压制射击/狂野射击。
`火力倾泻`（`WithHitCount(X*2)`）没有 Repeat 变量，只吃实际 +1 段。

### 8.4 寂寞体系的定论（重要）
- **寂寞不会自然结束**：`PowerStackType.Counter` 不自动衰减，只有羁绊回正才解除 → 进得去出不来
- 因此把「RABBIT1，开始清扫」做成**奥义牌**：打出 +1 羁绊 → 主动退出寂寞；
  并给它 **虚无**，逼玩家当回合就做抉择（不留到后面）
- `獠牙` 打出时 **+3 能量**（否则 −8 敏捷下能量已垫在防御上，打不出 3 费的清扫）
- `獠牙` 的 99 层脆弱**保留**：作者明确对标《33 号远征队》玛埃尔的司汤达症（高爆发+自破防御）
- 抵消脆弱需要多少羁绊（先加法后乘法，`Hook.ModifyBlock` 顺序）：防御 5→2层、备战8→3层、小憩13→5层

### 8.5 其他机制改动
- `省着点用`：改为**每回合内每消耗 3 张抽 1 张**，计数在 `AfterPlayerTurnStart` 清零（不跨回合累积）
- `战地医疗处理`：升级从“减费”改为**去掉消耗**
- `配备` 关键词文案：`若是由[gold]生成[/gold]获得，则本回合内免费打出`
- 作者自调（勿改）：`无人机校准` 触发 3 次→2 次、`小伤而已` 3费→2费

### 8.6 引擎踩坑记录
- **手牌被动与虚无互斥**：`CombatManager.DoTurnEnd` 里是 `if (HasTurnEndInHandEffect) … else if (Ethereal)`，
  实现了手牌被动的牌**虚无永远不会触发**。虚无要配别的效果请走 `AfterCardDrawn`（原版 `Void` 就这么做）
- 引擎**没有暴击系统**，做暴击得自己建，且 `ModifyDamageMultiplicative` 会被多次调用（预览/每段/联机），
  掷骰会导致预览与实际不一致 → 不推荐

### 8.7 仍未做
- 寂寞期回费（`LonelinessPower` 挂 `AfterEnergyReset` +1 能量）／寂寞期固定减伤（评价：暂时不必，缓冲已覆盖）
- 「+1 段」仍会让多段牌多吃一次力量（要根治得改伤害管线）
- 美术：9 张新卡卡图 + `please_protect_me_power` / `stay_away_power` 等 power 图标
