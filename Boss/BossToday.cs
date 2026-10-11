namespace BossUp.Boss
{
    /// <summary>
    /// Today's boss plan, kept on the host only. Deliberately not an ECS component so it can never
    /// end up in a save file: removing the mod must never break a save.
    /// </summary>
    internal static class BossToday
    {
        public static bool IsBossDay;

        /// <summary>Planned arrival as a fraction of the day (1 = closing time for end-of-day bosses).</summary>
        public static float ArrivalTime = -1f;

        public static bool Arrived;

        /// <summary>Time of day the boss actually arrived.</summary>
        public static float ArrivedAt = -1f;

        public static void Clear()
        {
            IsBossDay = false;
            ArrivalTime = -1f;
            Arrived = false;
            ArrivedAt = -1f;
        }
    }
}
