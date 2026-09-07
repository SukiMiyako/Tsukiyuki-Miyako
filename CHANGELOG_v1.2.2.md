# Tsukiyuki Miyako v1.2.2 更新日志

## 🔧 多分支兼容性更新

- 适配公开版 v0.107.1 与 BaseLib 3.4.5
- 所有攻击卡改用 `FromCardCompatibility`，兼容 public / public-beta 分支
- 修复 `OrbCmd.Passive` 在 public / public-beta 签名不同导致的 `MissingMethodException`
- 修复 `CreatureCmd.LoseBlock` 的分支差异
- 修复闪光无人机 `Trigger()` 的分支差异
- `Rabbit31SMG` 伤害加成改为双分支兼容的 Harmony 补丁
- BaseLib 最低依赖版本提升至 3.4.5

## ⚖️ 卡牌调整

- 战地医疗处理：升级效果由“获得固有”改为费用 -1
