using System.Reflection;
using HarmonyLib;
using KitchenLib;
using KitchenLib.Logging;
using KitchenMods;
using UnityEngine;

namespace BossUp
{
    public class Mod : BaseMod, IModSystem
    {
        public const string MOD_GUID = "com.hippox.bossup";
        public const string MOD_NAME = "BossUp!";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "HippoX";
        public const string MOD_GAMEVERSION = ">=1.3.0";

        internal static KitchenLogger Logger;
        internal static Harmony Harmony;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
#if DEBUG
            Logger.LogWarning($"{MOD_NAME} v{MOD_VERSION} DEBUG build loaded. F7 spawns a boss group. Do not upload this build.");
#else
            Logger.LogInfo($"{MOD_NAME} v{MOD_VERSION} loaded");
#endif
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Logger = InitLogger();
            Logger.LogInfo($"Game {Application.version}");

            Harmony = new Harmony(MOD_GUID);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            BossSettings.Register();
        }
    }
}
