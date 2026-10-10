using System.Linq;
using BossUp.Components;
using Kitchen;
using KitchenData;
using Unity.Entities;
using UnityEngine;

namespace BossUp.Boss
{
    internal static class BossSpawner
    {
        /// <summary>
        /// Spawns a boss group using the game's own group creation, so it walks in, queues and
        /// seats exactly like a normal group. Host only.
        /// </summary>
        public static Entity SpawnBossGroup(World world, EntityManager em)
        {
            var creator = world.GetExistingSystem<CreateCustomerGroup>();
            if (creator == null)
            {
                Mod.Logger.LogWarning("CreateCustomerGroup system not found, cannot spawn boss");
                return Entity.Null;
            }

            CustomerType generic = GameData.Main.Get<CustomerType>().FirstOrDefault(t => t.IsGenericGroup);
            if (generic == null)
            {
                Mod.Logger.LogWarning("No generic customer type found, cannot spawn boss");
                return Entity.Null;
            }

            int size = PickGroupSize(em);
            Entity group = creator.NewGroup(generic, size, false);
            em.AddComponent<CBossGroup>(group);

            Mod.Logger.LogInfo($"Boss group spawned ({size} member{(size == 1 ? "" : "s")})");
            return group;
        }

        private static int PickGroupSize(EntityManager em)
        {
            if (BossSettings.Party == BossParty.SingleBoss)
                return 1;

            int min = 1;
            int max = 2;
            using (var analysis = em.CreateEntityQuery(typeof(CreateCustomerSchedule.SAnalysis)))
            {
                if (!analysis.IsEmpty)
                {
                    var a = analysis.GetSingleton<CreateCustomerSchedule.SAnalysis>();
                    min = Mathf.Max(1, a.SmallestGroupPossibility);
                    max = Mathf.Max(min, a.LargestGroupPossibility);
                }
            }

            // A group bigger than the largest table would never be seated.
            using (var tables = em.CreateEntityQuery(typeof(SLargestTableSize)))
            {
                if (!tables.IsEmpty)
                {
                    int largest = tables.GetSingleton<SLargestTableSize>().LargestTableSize;
                    if (largest > 0)
                        max = Mathf.Min(max, largest);
                }
            }

            min = Mathf.Min(min, max);
            return Random.Range(min, max + 1);
        }
    }
}
