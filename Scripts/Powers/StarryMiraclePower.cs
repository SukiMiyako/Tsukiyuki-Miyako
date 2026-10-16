using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// 「星空奇迹」——「那片星空，名为奇迹」授予的能力。
///
/// 效果：**从卡牌中获得的师生羁绊 +1 层**。
///
/// 走的是 PowerCmd.Apply 的 "Given" 修正钩子
/// （Hook.ModifyPowerAmountGivenAdditive，PowerCmd.cs:125），
/// 是纯粹的数值修正 —— **不会额外制造一次"获得羁绊"事件**，
/// 所以不会重复触发「温柔依赖症」（获得羁绊→+1能量）
/// 和「与你共赏夜景」（获得羁绊→抽牌）。
///
/// 为什么把旧的"每打出 1 张能力牌 +1 羁绊"换掉：
///   旧效果是一条闭环 —— 打出能力牌 → +1 羁绊 → +1 能量 + 抽 1 张
///   → 抽到的又是能力牌 → 再打出……每张能力牌都免费且自替换；
///   叠上「We are Rabbits!」（所有能力牌费用 -1）后连能量都不需要，
///   等于把整库能力牌免费打完 + 把牌库抽干 + 羁绊爆表。
///   改成"卡牌给羁绊时多给 1 层"之后源头没了：收益被绑死在牌的数量上，
///   不再自我复制。
///
/// 已知不一致（待决定）：只有把卡牌当 applier/cardSource 传进来的牌吃得到加成。
/// 现在 SneakStart / TeamPrepare / OrigDream / OrigDreamUpgraded 这四张传的是 null，
/// 所以它们吃不到；要统一的话把它们改成传 this 就行。
/// </summary>
public sealed class StarryMiraclePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    // 布尔型加成：不再叠层、图标不显示数字
    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";

    public override string? CustomBigIconPath => $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    public override decimal ModifyPowerAmountGivenAdditive(
        PowerModel power,
        Creature giver,
        decimal amount,
        Creature? target,
        CardModel? cardSource)
    {
        // 只认"自己给自己加羁绊"这一种场合
        if (giver != base.Owner)
        {
            return 0m;
        }
        if (target != base.Owner)
        {
            return 0m;
        }
        if (power is not SenseiPower)
        {
            return 0m;
        }
        if (amount <= 0m)
        {
            // 削减羁绊的牌（光明正大的偷看 / 潮汐思念 / 帮大忙了！老师！）不吃加成
            return 0m;
        }

        // 注意：这里**不再要求 cardSource != null**。
        // 原因：本 mod 里给羁绊的牌有一部分把 cardSource 传成了 null
        // （偷跑开始 / 小队整备 / OrigDream / OrigDreamUpgraded），
        // 之前那道判断会让这几张完全吃不到 +1（报过的 bug）。
        // 现在全项目给羁绊的来源都是卡牌，所以直接用"是不是 SenseiPower 的正面变化"来判定就够。
        return 1m;
    }
}
