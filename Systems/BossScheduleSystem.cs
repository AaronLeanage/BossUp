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
        private bool _isBossDay;
        private bool _done;
        private bool _countdownAnnounced;

        /// <summary>During day: the time of day (0 to 1) the boss arrives.</summary>
        private float _arrivalTimeOfDay = -1f;

        /// <summary>End of day: seconds since the day began that the boss arrives, once the countdown has started.</summary>
        private float _arrivalSeconds = -1f;

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
                return;
            }

            int day = GetOrDefault<SDay>().Day;
            if (day != _day)
                StartDay(day);

            if (!_isBossDay || _done || !BossSettings.Enabled)
                return;

            if (BossSettings.Arrival == BossArrival.DuringDay)
                UpdateDuringDay(time);
            else
                UpdateEndOfDay(time);
        }

        private void StartDay(int day)
        {
            _day = day;
            _done = false;
            _countdownAnnounced = false;
            _arrivalSeconds = -1f;

            int every = Mathf.Max(1, BossSettings.EveryXDays);
            _isBossDay = BossSettings.Enabled && day > 0 && day % every == 0;
            _arrivalTimeOfDay = Random.Range(0.3f, 0.7f);

            if (_isBossDay)
                Mod.Logger.LogInfo($"Day {day} is a boss day ({BossSettings.Arrival})");
        }

        private void UpdateDuringDay(STime time)
        {
            float countdownStart = _arrivalTimeOfDay - BossSettings.CountdownSeconds / Mathf.Max(1f, time.DayLength);
            if (time.TimeOfDayUnbounded >= countdownStart)
                AnnounceCountdown();

            if (time.TimeOfDayUnbounded >= _arrivalTimeOfDay)
                Spawn();
        }

        private void UpdateEndOfDay(STime time)
        {
            // The countdown starts once the last normal group of the day has arrived.
            if (_arrivalSeconds < 0f && _scheduledCustomers.IsEmpty && time.TimeOfDayUnbounded > 0.25f)
            {
                _arrivalSeconds = time.SecondsSinceDayBegan + BossSettings.CountdownSeconds;
                AnnounceCountdown();
            }

            if (_arrivalSeconds < 0f)
                return;

            // If everyone has left after closing time, the day would end this frame. Arrive now instead.
            bool dayAboutToEnd = time.TimeOfDayUnbounded >= 1f && _customerGroups.IsEmpty;
            if (time.SecondsSinceDayBegan >= _arrivalSeconds || dayAboutToEnd)
                Spawn();
        }

        private void AnnounceCountdown()
        {
            if (_countdownAnnounced || BossSettings.CountdownSeconds <= 0)
                return;

            _countdownAnnounced = true;
            // TODO: step 8 replaces this with the on-screen countdown.
            Mod.Logger.LogInfo($"Boss arriving in {BossSettings.CountdownSeconds}s");
        }

        private void Spawn()
        {
            _done = true;
            BossSpawner.SpawnBossGroup(World, EntityManager);
        }
    }
}
