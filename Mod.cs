using System.Reflection;
using BossUp.Boss;
using BossUp.Views;
using HarmonyLib;
using Kitchen;
using KitchenLib;
using KitchenLib.Logging;
using KitchenLib.Utils;
using KitchenMods;
using UnityEngine;
using KitchenLogger = KitchenLib.Logging.KitchenLogger;

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
        internal static BossBonusText BossBonusText;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            Logger.LogInfo($"{MOD_NAME} v{MOD_VERSION} loaded");
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Logger = InitLogger();
            Logger.LogInfo($"Game {Application.version}");

            Harmony = new Harmony(MOD_GUID);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            BossSettings.Register();

            BossBonusText = AddGameDataObject<BossBonusText>();

            ViewUtils.RegisterView(BossTimelineView.VIEW_TYPE_NAME, typeof(SBossTimelineView), typeof(BossTimelineView), ViewMode.Screen, Vector3.zero);
        }
    }
}
