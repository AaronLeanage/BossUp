using BossUp.Views;
using HarmonyLib;
using Kitchen;
using UnityEngine;

namespace BossUp.Patches
{
    /// <summary>Gives the boss timeline view a prefab. It's an empty object, since the view draws nothing itself.</summary>
    [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
    internal static class BossTimelineViewPrefabPatch
    {
        private static GameObject _prefab;

        [HarmonyPrefix]
        private static bool Prefix(ViewType view_type, ref GameObject __result)
        {
            if (view_type != BossTimelineView.ViewType)
                return true;

            if (_prefab == null)
            {
                _prefab = new GameObject("BossUp Timeline View");
                _prefab.AddComponent<BossTimelineView>();
            }

            __result = _prefab;
            return false;
        }
    }

    /// <summary>Redraws the boss marker whenever the game updates its day timeline.</summary>
    [HarmonyPatch(typeof(TimeDisplayView), "UpdateData")]
    internal static class TimeDisplayViewPatch
    {
        [HarmonyPostfix]
        private static void Postfix(TimeDisplayView __instance, TimeDisplayView.ViewData view_data)
        {
            BossTimelineMarker.OnTimeDisplayUpdated(__instance, view_data);
        }
    }
}
