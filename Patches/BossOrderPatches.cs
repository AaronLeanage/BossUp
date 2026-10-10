using System.Collections.Generic;
using BossUp.Boss;
using BossUp.Components;
using HarmonyLib;
using Kitchen;
using KitchenData;
using Unity.Collections;
using Unity.Entities;

namespace BossUp.Patches
{
    /// <summary>
    /// Makes boss groups order everything on the menu.
    ///
    /// The game asks <see cref="AssignMenuRequests.AddItemToGroupMember"/> once per order request,
    /// and that picks a random menu item via <see cref="AssignMenuRequests.PickRandomMenuItem"/>.
    /// For a boss group, the first request seen in a phase is expanded into one order per menu
    /// item for every member (mains also get every side). Each order goes back through the
    /// game's own method with the pick forced, so variants, extras and add-on items behave
    /// exactly as normal. Any further requests from that group in the same pass are dropped.
    /// </summary>
    [HarmonyPatch(typeof(AssignMenuRequests))]
    internal static class BossOrderPatches
    {
        private static readonly HashSet<(Entity, MenuPhase)> _handled = new HashSet<(Entity, MenuPhase)>();
        private static int _forcedIndex = -1;
        private static bool _expanding;

        [HarmonyPatch("OnUpdate")]
        [HarmonyPrefix]
        private static void OnUpdate_Prefix()
        {
            _handled.Clear();
        }

        /// <summary>
        /// Order encouragers and Twitch orders pick a specific item instead of the random path.
        /// Bosses always take the random path so the expansion below runs.
        /// </summary>
        [HarmonyPatch("AttemptOrderSpecific")]
        [HarmonyPrefix]
        private static bool AttemptOrderSpecific_Prefix(AssignMenuRequests __instance, Entity group, ref bool __result)
        {
            if (_expanding || !__instance.EntityManager.HasComponent<CBossGroup>(group))
                return true;

            __result = false;
            return false;
        }

        [HarmonyPatch(nameof(AssignMenuRequests.PickRandomMenuItem))]
        [HarmonyPrefix]
        private static bool PickRandomMenuItem_Prefix(ref int __result)
        {
            if (_forcedIndex < 0)
                return true;

            __result = _forcedIndex;
            return false;
        }

        [HarmonyPatch(nameof(AssignMenuRequests.AddItemToGroupMember))]
        [HarmonyPrefix]
        private static bool AddItemToGroupMember_Prefix(AssignMenuRequests __instance, EntityContext ctx, Entity group,
            NativeArray<Entity> menu_entities, NativeArray<CMenuItem> menu_items, MenuPhase phase, ref float bonus_time)
        {
            if (_expanding)
                return true;

            EntityManager em = __instance.EntityManager;
            if (!em.HasComponent<CBossGroup>(group))
                return true;

            // Sides are ordered together with mains below, so the game's own random side requests are dropped.
            if (phase == MenuPhase.Side)
                return false;

            if (!_handled.Add((group, phase)))
                return false;

            int members = em.GetBuffer<CGroupMember>(group).Length;
            MenuPhase[] phases = phase == MenuPhase.Main
                ? new[] { MenuPhase.Main, MenuPhase.Side }
                : new[] { phase };
            int cap = BossSettings.MaxOrdersPerPhase;
            int ordered = 0;

            _expanding = true;
            try
            {
                for (int member = 0; member < members; member++)
                {
                    foreach (MenuPhase p in phases)
                    {
                        foreach (int index in BossMenu.ItemsForPhase(menu_items, p, cap))
                        {
                            _forcedIndex = index;
                            __instance.AddItemToGroupMember(ctx, group, member, menu_entities, menu_items, p, ref bonus_time);
                            ordered++;
                        }
                    }
                }
            }
            finally
            {
                _expanding = false;
                _forcedIndex = -1;
            }

            // A normal group orders roughly one item per member per phase. Every item beyond that
            // adds a fraction of a full wait-for-food bar, set by the Patience scaling preference.
            int extra = ordered - members;
            if (extra > 0)
            {
                var settings = em.GetComponentData<CCustomerSettings>(group);
                bonus_time += extra * BossSettings.PatienceScaling * settings.Patience.WaitForFood;
            }

            Mod.Logger.LogInfo($"Boss group ordered {ordered} item(s) for {phase} phase");
            return false;
        }
    }
}
