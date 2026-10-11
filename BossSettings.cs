using System.Linq;
using PreferenceSystem;

namespace BossUp
{
    public enum BossArrival
    {
        DuringDay = 0,
        EndOfDay = 1
    }

    public enum BossParty
    {
        SingleCustomer = 0,
        FullGroup = 1
    }

    public enum BossOrderMode
    {
        MenuItems = 0,
        IncludeVariants = 1
    }

    /// <summary>
    /// Host-only boss settings. Game simulation runs on the host, so only the host's values
    /// are ever read. Clients see the menu but it is blocked for them.
    /// </summary>
    public static class BossSettings
    {
        private const string ENABLED = "Enabled";
        private const string EVERY_X_DAYS = "EveryXDays";
        private const string ARRIVAL = "Arrival";
        private const string PARTY = "Party";
        private const string ORDER_MODE = "OrderMode";
        private const string MAX_ORDERS_PER_PHASE = "MaxOrdersPerPhase";
        private const string PATIENCE_SCALING = "PatienceScaling";
        private const string BONUS_MULTIPLIER = "BonusMultiplier";
        private const string TIMELINE_MARKER = "TimelineMarker";

        private static PreferenceSystemManager _prefs;

        public static bool Enabled => _prefs.Get<bool>(ENABLED);
        public static int EveryXDays => _prefs.Get<int>(EVERY_X_DAYS);
        public static BossArrival Arrival => (BossArrival)_prefs.Get<int>(ARRIVAL);
        public static BossParty Party => (BossParty)_prefs.Get<int>(PARTY);
        public static BossOrderMode OrderMode => (BossOrderMode)_prefs.Get<int>(ORDER_MODE);
        /// <summary>Max orders per member per phase. 0 means unlimited.</summary>
        public static int MaxOrdersPerPhase => _prefs.Get<int>(MAX_ORDERS_PER_PHASE);
        /// <summary>Extra WaitForFood patience bars granted per extra item ordered.</summary>
        public static float PatienceScaling => _prefs.Get<float>(PATIENCE_SCALING);
        /// <summary>Multiplier applied to the normal reward of every dish served to a boss.</summary>
        public static float BonusMultiplier => _prefs.Get<float>(BONUS_MULTIPLIER);
        /// <summary>Show the boss's arrival time on the day timeline, like rushes.</summary>
        public static bool TimelineMarker => _prefs.Get<bool>(TIMELINE_MARKER);

        internal static void Register()
        {
            int[] days = Enumerable.Range(1, 15).ToArray();
            string[] dayStrings = days.Select(d => d == 1 ? "Every day" : $"Every {d} days").ToArray();

            _prefs = new PreferenceSystemManager(Mod.MOD_GUID, Mod.MOD_NAME);
            // Split into submenus so no page runs off the bottom of the screen. Every level is
            // blocked for clients, since only the host's values are used.
            _prefs
                .AddLabel("BossUp! (host only)")
                .AddHostStatusConditionalBlocker()

                .AddLabel("Bosses")
                .AddOption(ENABLED, true, new[] { false, true }, new[] { "Off", "On" })

                .AddLabel("Boss marker on timeline")
                .AddOption(TIMELINE_MARKER, true, new[] { false, true }, new[] { "Off", "On" })

                .AddSubmenu("Schedule", "Schedule")
                    .AddLabel("Schedule (host only)")
                    .AddHostStatusConditionalBlocker()
                    .AddLabel("Boss frequency")
                    .AddOption(EVERY_X_DAYS, 15, days, dayStrings)
                    .AddLabel("Arrival")
                    .AddOption(ARRIVAL, (int)BossArrival.EndOfDay,
                        new[] { (int)BossArrival.DuringDay, (int)BossArrival.EndOfDay },
                        new[] { "During day", "End of day" })
                    .ConditionalBlockerDone()
                    .AddSpacer()
                .SubmenuDone()

                .AddSubmenu("Orders", "Orders")
                    .AddLabel("Orders (host only)")
                    .AddHostStatusConditionalBlocker()
                    .AddLabel("Boss party")
                    .AddOption(PARTY, (int)BossParty.SingleCustomer,
                        new[] { (int)BossParty.SingleCustomer, (int)BossParty.FullGroup },
                        new[] { "Single customer", "Full group" })
                    .AddLabel("Orders")
                    .AddOption(ORDER_MODE, (int)BossOrderMode.MenuItems,
                        new[] { (int)BossOrderMode.MenuItems, (int)BossOrderMode.IncludeVariants },
                        new[] { "Menu items", "Include variants" })
                    .AddLabel("Max orders per phase")
                    .AddOption(MAX_ORDERS_PER_PHASE, 8, new[] { 4, 8, 12, 0 }, new[] { "4", "8", "12", "Unlimited" })
                    .ConditionalBlockerDone()
                    .AddSpacer()
                .SubmenuDone()

                .AddSubmenu("Difficulty & rewards", "DifficultyRewards")
                    .AddLabel("Difficulty & rewards (host only)")
                    .AddHostStatusConditionalBlocker()
                    .AddLabel("Patience scaling")
                    .AddOption(PATIENCE_SCALING, 0.5f, new[] { 0.25f, 0.5f, 1f }, new[] { "Low", "Medium", "High" })
                    .AddLabel("Bonus multiplier")
                    .AddOption(BONUS_MULTIPLIER, 3f,
                        new[] { 1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f },
                        new[] { "1x", "1.5x", "2x", "2.5x", "3x", "4x", "5x" })
                    .ConditionalBlockerDone()
                    .AddSpacer()
                .SubmenuDone()


                .ConditionalBlockerDone()
                .AddSpacer()
                .AddSpacer();

            _prefs.RegisterMenu(PreferenceSystemManager.MenuType.MainMenu);
            _prefs.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }
    }
}
