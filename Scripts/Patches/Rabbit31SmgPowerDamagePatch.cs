using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TsukiyukiMiyako.Scripts.Powers;

namespace TsukiyukiMiyako.Scripts.Patches;

/// <summary>
/// Keeps Rabbit31SmgPower's flat SMG damage bonus working on both the public main branch
/// and the public-beta branch. The game moved <c>ModifyDamageAdditive</c> between versions
/// by adding a <see cref="CardPlay"/> parameter, so a normal override would be version-specific.
/// Instead, intercept whichever overload exists at runtime.
///
/// ★ 只给**单段** SMG 牌加这份平坦伤害：多段牌在 <see cref="Rabbit31SmgPower.ModifyAttackHitCount"/>
/// 里已经拿过"额外命中 1 次"，这里再加就两头吃。判据是 power 上的 `MultiHitCard` 标记
/// （由命中数钩子在同一段 `AttackCommand.Execute` 里先行写入）。
/// </summary>
public static class Rabbit31SmgPowerDamagePatch
{
    private static bool HandleDamage(AbstractModel power, Creature? dealer, CardModel? cardSource, out decimal result)
    {
        result = 0m;

        if (power is not Rabbit31SmgPower rabbitPower)
        {
            return false;
        }

        if (rabbitPower.Owner != dealer || cardSource == null)
        {
            return true;
        }

        if (!cardSource.CanonicalKeywords.Contains(MyKeywords.Smg))
        {
            return true;
        }

        // 多段牌已获得"额外命中 1 次"，不再叠加平坦伤害
        if (rabbitPower.IsMultiHitAttack(cardSource))
        {
            return true;
        }

        result = rabbitPower.Amount;
        return true;
    }

    [HarmonyPatch]
    private static class OldModifyDamageAdditivePatch
    {
        private static MethodInfo? Target => AccessTools.Method(
            typeof(AbstractModel),
            nameof(AbstractModel.ModifyDamageAdditive),
            [
                typeof(Creature),
                typeof(decimal),
                typeof(ValueProp),
                typeof(Creature),
                typeof(CardModel)
            ]);

        private static bool Prepare()
        {
            return Target != null;
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            if (Target != null)
            {
                yield return Target;
            }
        }

        private static bool Prefix(
            AbstractModel __instance,
            Creature? target,
            decimal amount,
            ValueProp props,
            Creature? dealer,
            CardModel? cardSource,
            ref decimal __result)
        {
            return !HandleDamage(__instance, dealer, cardSource, out __result);
        }
    }

    [HarmonyPatch]
    private static class NewModifyDamageAdditivePatch
    {
        private static MethodInfo? Target => AccessTools.Method(
            typeof(AbstractModel),
            nameof(AbstractModel.ModifyDamageAdditive),
            [
                typeof(Creature),
                typeof(decimal),
                typeof(ValueProp),
                typeof(Creature),
                typeof(CardModel),
                typeof(CardPlay)
            ]);

        private static bool Prepare()
        {
            return Target != null;
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            if (Target != null)
            {
                yield return Target;
            }
        }

        private static bool Prefix(
            AbstractModel __instance,
            Creature? target,
            decimal amount,
            ValueProp props,
            Creature? dealer,
            CardModel? cardSource,
            CardPlay? cardPlay,
            ref decimal __result)
        {
            return !HandleDamage(__instance, dealer, cardSource, out __result);
        }
    }
}
