using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Tsukiyuki_Miyako.MiyakoModCode.Character;
using TsukiyukiMiyako.Scripts;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// RABBIT-31 式冲锋枪的专武增益，按"这张 SMG 牌本来打几段"分两种给法：
///
///   · **本来就多段**（如扫射 3×3、冲锋爆发 5 段）
///     → 额外命中 **1 次**（走 <see cref="ModifyAttackHitCount"/>）
///   · **单段**（如看准再打、掠夺射击）
///     → 伤害 **+3**（走 <see cref="Rabbit31SmgPowerDamagePatch"/> 里的平坦加成）
///
/// 为什么这么拆：+1 段对单段高伤牌等于伤害翻倍（看准再打封顶 40 → 80），
/// 对多段牌只是 3→4 段这种幅度。分开给才能既保住"连发"的手感，
/// 又不让单发重击牌被这条增益带飞。
///
/// 钩子说明：
///   - `ModifyAttackHitCount(AttackCommand, int)` 是引擎现成的
///     （`AbstractModel.cs:1411`，在 `AttackCommand.Execute` 里第 537 行被调用），
///     `AttackCommand.ModelSource` 能直接拿到出牌的卡。
///   - 判多段用的是**传进来的当前段数**，而不是查 `Repeat` 变量 ——
///     「火力倾泻」用 `WithHitCount(X*2)` 现算段数、根本没有 `Repeat` 变量，
///     查变量会把它误判成单段。
///
/// `_multiHitCardId` 是给伤害补丁看的标记：`ModifyAttackHitCount` 在伤害结算之前跑
/// （同一段 `Execute` 内），所以伤害发生时标记已经就位；`AfterAttack` 里清掉。
/// </summary>
public sealed class Rabbit31SmgPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    // 层数 = 单段牌吃到的平坦伤害加成
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath =>
        $"res://Tsukiyuki Miyako/images/powers/{Id.Entry.ToLowerInvariant()}.png";

    public override string? CustomBigIconPath =>
        $"res://Tsukiyuki Miyako/images/powers/big/{Id.Entry.ToLowerInvariant()}.png";

    /// <summary>
    /// 本次攻击被判定为"多段 SMG"的卡牌 Id；伤害补丁据此跳过平坦加成。
    ///
    /// 用 **static** 字段而不是实例字段：钩子和 Harmony 补丁拿到的可能是不同的
    /// power 实例（canonical / mutable 之别），实例字段会静默失效 ——
    /// 那正是"多段牌同时吃到额外命中和平坦伤害"的原因。
    /// 存 `ModelId` 而不是对象引用同理：`ModelId` 是 record，按 Category+Entry 值相等。
    /// </summary>
    private static ModelId _multiHitCardId = ModelId.none;

    public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
    {
        if (attack.ModelSource is not CardModel card)
        {
            return hitCount;
        }

        // 只对"自己打出的 SMG 牌"生效
        if (card.Owner?.Creature != Owner || !card.CanonicalKeywords.Contains(MyKeywords.Smg))
        {
            return hitCount;
        }

        // 单段牌不在这里加成（它们拿的是 +3 伤害）
        if (hitCount < 2)
        {
            return hitCount;
        }

        _multiHitCardId = card.Id;
        return hitCount + 1;
    }

    public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        _multiHitCardId = ModelId.none;
        return Task.CompletedTask;
    }

    /// <summary>伤害补丁用：这张卡本次攻击是否已经拿过"多打一段"。</summary>
    internal bool IsMultiHitAttack(CardModel card)
    {
        if (_multiHitCardId != ModelId.none && card.Id == _multiHitCardId)
        {
            return true;
        }

        // 兜底：静态多段牌（带 Repeat ≥ 2 变量）一律按多段处理，
        // 即使上面的标记因为钩子实例差异没写进来，也不会两头吃。
        return card.DynamicVars.ContainsKey("Repeat") && card.DynamicVars["Repeat"].IntValue >= 2;
    }
}
