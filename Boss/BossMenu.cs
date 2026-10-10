using System.Collections.Generic;
using Kitchen;
using KitchenData;
using Unity.Collections;
using UnityEngine;

namespace BossUp.Boss
{
    internal static class BossMenu
    {
        /// <summary>
        /// Indices into the active menu of every distinct item in <paramref name="phase"/>,
        /// randomly trimmed to <paramref name="cap"/> when a cap is set (0 = unlimited).
        /// </summary>
        public static List<int> ItemsForPhase(NativeArray<CMenuItem> menuItems, MenuPhase phase, int cap)
        {
            var seen = new HashSet<int>();
            var indices = new List<int>();
            for (int i = 0; i < menuItems.Length; i++)
            {
                if (menuItems[i].Phase == phase && seen.Add(menuItems[i].Item))
                    indices.Add(i);
            }

            if (cap > 0 && indices.Count > cap)
            {
                for (int i = indices.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (indices[i], indices[j]) = (indices[j], indices[i]);
                }
                indices.RemoveRange(cap, indices.Count - cap);
            }

            return indices;
        }
    }
}
