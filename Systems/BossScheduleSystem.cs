using BossUp.Boss;
using Kitchen;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace BossUp.Systems
{
    /// <summary>
    /// Decides when today's boss group arrives and spawns it. Host only, restaurant only.
    ///
    /// Nothing here is saved: whether today is a boss day comes from the day number, and the game
    /// only saves between days, so a boss can never be half-way through when a save happens.
    ///
    /// Runs before <see cref="AdvanceTime"/>, which ends the day once closing time has passed and no
    /// customer groups remain. Spawning first means an end-of-day boss can always hold the day open.
    /// </summary>
    [UpdateInGroup(typeof(TimeManagementGroup))]
    [UpdateBefore(typeof(AdvanceTime))]
    public class BossScheduleSystem : RestaurantSystem, IModSystem
    {
        private EntityQuery _scheduledCustomers;
        private EntityQuery _customerGroups;

        private int _day = -1;

        protected override void Initialise()
        {
            base.Initialise();
            _scheduledCustomers = GetEntityQuery(typeof(CScheduledCustomer));
            _customerGroups = GetEntityQuery(typeof(CCustomerGroup));
        }

        protected override void OnUpdate()
        {
            if (Has<SPracticeMode>() || !Has<SIsDayTime>() || !Require(out STime time))
            {
                _day = -1;
                BossToday.Clear();
                return;
            }

            int day = GetOrDefault<SDay>().Day;
            if (day != _day)
                StartDay(day);

            if (!BossToday.IsBossDay || BossToday.Arrived || !BossSettings.Enabled)
                return;

            if (BossSettings.Arrival == BossArrival.DuringDay)
                UpdateDuringDay(time);
            else
                UpdateEndOfDay(time);
        }

        private void StartDay(int day)
        {
            _day = day;
            BossToday.Clear();

            int every = Mathf.Max(1, BossSettings.EveryXDays);
            BossToday.IsBossDay = BossSettings.Enabled && day > 0 && day % every == 0;
            if (!BossToday.IsBossDay)
                return;

            // End-of-day bosses arrive after the last normal group, so their marker sits at closing time.
            BossToday.ArrivalTime = BossSettings.Arrival == BossArrival.DuringDay
                ? Random.Range(0.3f, 0.7f)
                : 1f;

            Mod.Logger.LogInfo($"Day {day} is a boss day ({BossSettings.Arrival}" +
                               (BossSettings.Arrival == BossArrival.DuringDay ? $", arriving {Mathf.RoundToInt(BossToday.ArrivalTime * 100f)}% through the day)" : ")"));
        }

        private void UpdateDuringDay(STime time)
        {
            if (time.TimeOfDayUnbounded >= BossToday.ArrivalTime)
                Spawn(time);
        }

        private void UpdateEndOfDay(STime time)
        {
            bool lastGroupArrived = _scheduledCustomers.IsEmpty && time.TimeOfDayUnbounded > 0.25f;
            // If everyone has left after closing time, the day would end this frame. Arrive now instead.
            bool dayAboutToEnd = time.TimeOfDayUnbounded >= 1f && _customerGroups.IsEmpty;
            if (lastGroupArrived || dayAboutToEnd)
                Spawn(time);
        }

        private void Spawn(STime time)
        {
            BossToday.Arrived = true;
            BossToday.ArrivedAt = time.TimeOfDay;
            Mod.Logger.LogInfo($"Boss arriving now ({Describe(time)})");
            BossSpawner.SpawnBossGroup(World, EntityManager);
        }

        private string Describe(STime time)
        {
            return $"day {Mathf.RoundToInt(time.TimeOfDayUnbounded * 100f)}% through, " +
                   $"{_scheduledCustomers.CalculateEntityCount()} normal group(s) still to arrive, " +
                   $"{_customerGroups.CalculateEntityCount()} group(s) in restaurant";
        }
    }
}
