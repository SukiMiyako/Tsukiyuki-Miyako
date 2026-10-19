using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts;

/// <summary>
/// 给多段 SMG 牌用的 `Repeat` 变量：**RABBIT-31 在场时，卡面显示的段数自动 +1**。
///
/// 为什么需要它：真正的"多打一段"是 <see cref="Rabbit31SmgPower.ModifyAttackHitCount"/>
/// 在结算时加的，它不会改动卡牌的 `DynamicVars`，于是卡面还印着旧数字（3 次 → 实际打 4 次），
/// 玩家会以为数值出错了。
///
/// 做法照 `CalculatedVar` 的路线：只重写**显示**相关的两个入口，不动 `BaseValue`：
///   · `UpdateCardPreview(...)` → 算给预览用的值；
///   · `GetBaseValueForIConvertible()` → 卡面 `{Repeat}` 文本真正读的就是这个。
/// 因为每次渲染都会重算，所以「牌进手牌 / 离手 / power 被移除」都会自动跟上，
/// 不需要记录、也不需要复原 —— 这正是选"只改显示"而不是"改运行时数据"的原因。
///
/// 判据与结算侧保持一致：单段牌（BaseValue &lt; 2）不显示 +1，因为它们拿的是 +3 伤害。
/// 段数动态计算、没有 `Repeat` 变量的牌（如「火力倾泻」`WithHitCount(X*2)`）用不到这个变量。
/// </summary>
public sealed class SmgRepeatVar : RepeatVar
{
    public SmgRepeatVar(int times) : base(times)
    {
    }

    /// <summary>当前是否应该多显示一段。</summary>
    private bool ShowExtraHit(CardModel? card)
    {
        // 单段牌不参与"额外命中"（它们吃的是平坦伤害）
        if (BaseValue < 2m)
        {
            return false;
        }

        if (card == null)
        {
            return false;
        }

        // ★ 必须先判可变：`CardModel.Owner` 的 getter 自带 `AssertMutable()`
        // （CardModel.cs:333-339），图鉴 / 奖励预览 / 变形预览渲染的都是 **canonical** 卡，
        // 直接读 `card.Owner` 会抛 CanonicalModelException（SmgSweep 那次就是这么炸的）。
        // canonical 卡上本来也不该有额外命中，直接返回 false。
        if (!card.IsMutable)
        {
            return false;
        }

        if (!card.CanonicalKeywords.Contains(MyKeywords.Smg))
        {
            return false;
        }

        Creature? creature = card.Owner?.Creature;
        return creature != null && creature.GetPowerAmount<Rabbit31SmgPower>() > 0;
    }

    public override void UpdateCardPreview(
        CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        // 每次都从 BaseValue 重算，避免多次刷新时累加
        PreviewValue = BaseValue + (ShowExtraHit(card) ? 1m : 0m);
    }

    /// <summary>卡面 `{Repeat}` 文本读取的入口（`DynamicVar.GetBaseValueForIConvertible`）。</summary>
    protected override decimal GetBaseValueForIConvertible()
    {
        return BaseValue + (ShowExtraHit(_owner as CardModel) ? 1m : 0m);
    }

    public override string ToString()
    {
        return GetBaseValueForIConvertible().ToString();
    }
}
