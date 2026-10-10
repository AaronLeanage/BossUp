#if DEBUG
using BossUp.Boss;
using Kitchen;
using KitchenMods;
using UnityEngine;

namespace BossUp.Systems
{
    /// <summary>
    /// Debug builds only. F7 spawns a boss group immediately during service (host only).
    /// Compiled out of Release builds so it can never ship.
    /// </summary>
    public class DebugBossHotkeys : RestaurantSystem, IModSystem
    {
        protected override void OnUpdate()
        {
            if (!Input.GetKeyDown(KeyCode.F7))
                return;

            if (!Has<SIsDayTime>())
            {
                Mod.Logger.LogInfo("F7 ignored: bosses can only be spawned during service");
                return;
            }

            BossSpawner.SpawnBossGroup(World, EntityManager);
        }
    }
}
#endif
