using System.Collections.Generic;
using System.Linq;
using Kitchen;
using KitchenData;
using Unity.Collections;
using UnityEngine;

namespace BossUp.Boss
{
    /// <summary>One order a boss member will place.</summary>
    internal struct BossOrder
    {
        /// <summary>Index into the active menu.</summary>
        public int MenuIndex;

        /// <summary>When set, this exact variant is ordered. Otherwise the game picks a random one.</summary>
        public bool HasConfig;

        public ItemList Config;
    }

    internal static class BossMenu
    {
        /// <summary>Safety limit so a dish with lots of toppings can't produce an enormous list.</summary>
        private const int MAX_VARIANTS_PER_DISH = 64;

        /// <summary>
        /// Every order one boss member places in <paramref name="phase"/>.
        ///
        /// Each distinct menu item gives one order, or one per variant when <paramref name="includeVariants"/>
        /// is set. Orders are interleaved across dishes, so when <paramref name="cap"/> trims the list every
        /// dish still appears before any dish's second variant. If there are fewer orders than
        /// <paramref name="min"/>, the list repeats from the start until it gets there. The cap wins over
        /// the minimum. 0 means no cap / no minimum.
        /// </summary>
        public static List<BossOrder> OrdersForPhase(NativeArray<CMenuItem> menuItems, MenuPhase phase,
            NativeArray<CAvailableIngredient> unlocks, bool includeVariants, int min, int cap)
        {
            var dishes = new List<List<BossOrder>>();
            var seen = new HashSet<int>();
            for (int i = 0; i < menuItems.Length; i++)
            {
                int item = menuItems[i].Item;
                if (menuItems[i].Phase != phase || !seen.Add(item))
                    continue;

                var dish = new List<BossOrder>();
                if (includeVariants && GameData.Main.TryGet<ItemGroup>(item, out ItemGroup group))
                {
                    foreach (ItemList config in Variants(group, AvailableIngredients(unlocks, item)))
                        dish.Add(new BossOrder { MenuIndex = i, HasConfig = true, Config = config });
                }

                if (dish.Count == 0)
                    dish.Add(new BossOrder { MenuIndex = i });

                Shuffle(dish);
                dishes.Add(dish);
            }

            var orders = new List<BossOrder>();
            if (dishes.Count == 0)
                return orders;

            Shuffle(dishes);
            int longest = dishes.Max(d => d.Count);
            for (int round = 0; round < longest; round++)
            {
                foreach (List<BossOrder> dish in dishes)
                {
                    if (round < dish.Count)
                        orders.Add(dish[round]);
                }
            }

            int target = Mathf.Max(orders.Count, min);
            if (cap > 0)
                target = Mathf.Min(target, cap);

            if (target <= orders.Count)
            {
                orders.RemoveRange(target, orders.Count - target);
                return orders;
            }

            int distinct = orders.Count;
            for (int i = 0; orders.Count < target; i++)
                orders.Add(orders[i % distinct]);

            return orders;
        }

        /// <summary>The ingredients unlocked for <paramref name="menuItem"/>, same as the game uses when ordering.</summary>
        private static HashSet<int> AvailableIngredients(NativeArray<CAvailableIngredient> unlocks, int menuItem)
        {
            var available = new HashSet<int>();
            foreach (CAvailableIngredient unlock in unlocks)
            {
                if (unlock.MenuItem == menuItem)
                    available.Add(unlock.Ingredient);
            }
            return available;
        }

        /// <summary>
        /// Every configuration the game could randomly pick for this dish. Mirrors
        /// ItemSetView.GetRandomConfiguration: each set contributes between Min and Max of its items,
        /// and sets that need unlocking only offer unlocked items.
        /// </summary>
        private static List<ItemList> Variants(ItemGroup group, HashSet<int> available)
        {
            var results = new List<ItemList> { default };
            foreach (ItemGroup.ItemSet set in group.DerivedSets)
            {
                List<int> items = set.Items
                    .Where(item => item != null && (!set.RequiresUnlock || available.Contains(item.ID)))
                    .Select(item => item.ID)
                    .ToList();

                int lo = Mathf.Min(set.Min, items.Count);
                int hi = Mathf.Min(set.Max, items.Count);
                var choices = new List<List<int>>();
                for (int k = lo; k <= hi; k++)
                    AddCombinations(items, k, 0, new List<int>(), choices);

                var next = new List<ItemList>();
                foreach (ItemList partial in results)
                {
                    foreach (List<int> choice in choices)
                    {
                        if (next.Count >= MAX_VARIANTS_PER_DISH)
                            break;

                        ItemList config = partial;
                        foreach (int id in choice)
                            config.Add(id);
                        next.Add(config);
                    }
                }

                results = next;
            }

            return results;
        }

        private static void AddCombinations(List<int> items, int size, int start, List<int> current, List<List<int>> output)
        {
            if (current.Count == size)
            {
                output.Add(new List<int>(current));
                return;
            }

            for (int i = start; i < items.Count; i++)
            {
                current.Add(items[i]);
                AddCombinations(items, size, i + 1, current, output);
                current.RemoveAt(current.Count - 1);
            }
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
