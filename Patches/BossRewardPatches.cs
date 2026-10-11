using BossUp.Components;
using HarmonyLib;
using Kitchen;
using Unity.Entities;
using UnityEngine;

namespace BossUp.Patches
{
    /// <summary>
    /// Multiplies the reward of every dish served to a boss group by the Bonus multiplier.
    ///
    /// <see cref="GrantMoneyForSatisfactions.GrantReward"/> adds each dish's reward to the group's
    /// <see cref="CGroupReward"/>, which the game pays out when the group finishes a course. Topping
    /// up that same total means the bonus shows in the normal money popup and is only paid if the
    /// group is actually served.
    /// </summary>
    [HarmonyPatch(typeof(GrantMoneyForSatisfactions), nameof(GrantMoneyForSatisfactions.GrantReward))]
    internal static class BossRewardPatches
    {
        [HarmonyPrefix]
        private static void Prefix(GrantMoneyForSatisfactions __instance, Entity group, out int __state)
        {
            EntityManager em = __instance.EntityManager;
            __state = em.HasComponent<CBossGroup>(group) && em.HasComponent<CGroupReward>(group)
                ? em.GetComponentData<CGroupReward>(group).Amount
                : -1;
        }

        [HarmonyPostfix]
        private static void Postfix(GrantMoneyForSatisfactions __instance, EntityContext ctx, Entity group, int __state)
        {
            if (__state < 0)
                return;

            EntityManager em = __instance.EntityManager;
            CGroupReward reward = em.GetComponentData<CGroupReward>(group);
            int earned = reward.Amount - __state;
            int bonus = Mathf.RoundToInt(earned * (BossSettings.BonusMultiplier - 1f));
            if (bonus <= 0)
                return;

            reward.Amount += bonus;
            em.SetComponentData(group, reward);
            // Record it like any other flat bonus so the end-of-day breakdown adds up.
            CommitCompletedGroups.AddEvent(ctx, group, 0, bonus);
        }
    }
}
