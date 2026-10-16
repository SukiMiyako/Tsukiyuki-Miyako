using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using Tsukiyuki_Miyako.MiyakoModCode.Powers;

namespace TsukiyukiMiyako.Scripts.Powers;

/// <summary>
/// 「一步不退」——「我还不能倒下」授予的能力。**一次性**：挡下一次致命伤害后自己移除。
///
/// 机制落点：走引擎的**死亡层**钩子（AbstractModel.ShouldDie / AfterPreventingDeath），
/// 和原版 FairyInABottle（妖精之瓶）、LizardTail（蜥蜴尾巴）同一个挂载点。
///
/// 为什么用死亡层而不是掉血层（ModifyHpLostAfterOsty）：
///   - 死亡层能连 Doom 的即死（DoomPower.DoomKill → CreatureCmd.Kill，不带 force）
///     和事件处决（TabletOfTruth 同样是无 force 的 Kill）一起挡下来；
///   - 掉血层只能挡"伤害"，而且一旦挂上就会先触发，让死亡层永远轮不到，
///     反而丢掉 Doom / 事件处决的防护。
///
/// 引擎行为备忘（CreatureCmd.KillWithoutCheckingWinCondition）：
///   1. 先把血清到 0 → BeforeDeath → Hook.ShouldDie（问所有 listener，ShouldDie 一轮、
///      ShouldDieLate 一轮；**谁先返回 false 谁就是 preventer，立刻 return**，后面的不再问）；
///   2. 有 preventer → AfterDeath(wasRemovalPrevented:true) → AfterPreventingDeath(preventer)；
///   3. 紧接着 `if (creature.IsDead) Kill(recursion + 1)`，recursion >= 10 直接抛
///      "Combat is continually preventing the last creature from being killed!"。
///      —— 所以下面必须把血捞回 >0，否则只是把死亡推迟一帧。
///
/// 剩下挡不住的：`Kill(creature, force: true)`（放弃本局、Sandpit）；
/// `MaxHp &lt;= 0` 时引擎在 ShouldDie 之前就判定死亡（`force || MaxHp &lt;= 0 || ShouldDie(...)`）。
/// 因为是一次性，Doom 这种每回合都要杀你一次的机制可以靠它买一个回合，但买不了第二个。
/// </summary>
public sealed class NotOneStepBackPower : MiyakoModPower
{
    public override PowerType Type => PowerType.Buff;

    // 布尔型能力：不叠层、不显示数字
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldDie(Creature creature)
    {
        // 只保自己；其他生物（含敌人）照常死亡
        if (creature != base.Owner)
        {
            return true;
        }

        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();

        // 她的事件台词：「不行……我还不能在这里倒下。」
        // 直接复用 characters 表里已有的 <角色>.eventDeathPrevention，不用新增本地化。
        // 原版只在事件处决时用它，这里把它挪进战斗当作她硬撑到底的那一句。
        if (base.Owner.Player != null)
        {
            ThinkCmd.Play(base.Owner.Player.Character.EventDeathPreventionLine, base.Owner, 2.0);
        }

        // 名刀式结算：不是回血，是"把命收到 1 点"。
        // SetCurrentHp 自带复活动画（StartReviveAnim），且内部有兜底：设完还是死会自己再 Kill。
        await CreatureCmd.SetCurrentHp(creature, 1m);

        // 屏障破碎：在能力消失的同一帧发声。
        // 必须放在 Remove 之前 —— PowerCmd.Remove 内部有 Cmd.CustomScaledWait(0.2f, 0.4f)，
        // 写在它后面声音会晚 0.2~0.4 秒才响。
        // 音量：broke.mp3 本身录得偏轻（无 ReplayGain 标签），所以这里给 2 倍增益；
        //       嫌吵就往下调，还是太小就干脆把音频文件做一次响度归一化再换掉。
        SfxCmd.Play("res://Audio/broke.mp3", 3.0f);

        // 一次性：这一次守住了，能力就此用完。
        await PowerCmd.Remove(this);
    }
}
