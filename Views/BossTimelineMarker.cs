using System.Collections.Generic;
using HarmonyLib;
using Kitchen;
using TMPro;
using UnityEngine;

namespace BossUp.Views
{
    /// <summary>
    /// Client side: draws the boss marker on the game's day timeline (<see cref="TimeDisplayView"/>).
    ///
    /// The timeline has three rush markers fixed at 20%, 50% and 80% of the day. Each is a text label
    /// showing a glyph from the game's icon font ("O" is the rush icon), animated white to flashing
    /// yellow when the rush is due. The boss marker is a copy of one showing a target sprite instead, placed at
    /// the boss's arrival time by interpolating between the first and last rush marker positions.
    /// </summary>
    internal static class BossTimelineMarker
    {
        private static readonly AccessTools.FieldRef<TimeDisplayView, List<Animator>> RushMarkers =
            AccessTools.FieldRefAccess<TimeDisplayView, List<Animator>>("RushMarkers");

        private static readonly int IsTriggered = Animator.StringToHash("IsTriggered");

        // The game's "range" sprite (a target). tint=1 lets the marker's white / flashing yellow colour apply.
        private const string BOSS_ICON = "<sprite name=\"range\" tint=1>";

        private const float FIRST_RUSH_TIME = 0.2f;
        private const float LAST_RUSH_TIME = 0.8f;

        private static BossTimelineView.ViewData _state;
        private static TimeDisplayView _display;
        private static Animator _marker;
        private static float _timeOfDay;
        private static bool _isServiceTime;

        public static void SetState(BossTimelineView.ViewData state)
        {
            _state = state;
            Refresh();
        }

        public static void OnTimeDisplayUpdated(TimeDisplayView display, TimeDisplayView.ViewData data)
        {
            if (_display != display)
            {
                _display = display;
                _marker = null;
            }

            _timeOfDay = data.TimeOfDay;
            _isServiceTime = !data.IsNight && !data.IsPractice;
            Refresh();
        }

        private static void Refresh()
        {
            if (_display == null)
                return;

            bool show = _state.Show && _isServiceTime && _state.ArrivalTime >= 0f;
            if (_marker == null)
            {
                if (!show)
                    return;
                _marker = CreateMarker(_display);
                if (_marker == null)
                    return;
            }

            _marker.gameObject.SetActive(show);
            if (!show)
                return;

            List<Animator> rushMarkers = RushMarkers(_display);
            Vector3 first = rushMarkers[0].transform.localPosition;
            Vector3 last = rushMarkers[2].transform.localPosition;
            float t = (_state.ArrivalTime - FIRST_RUSH_TIME) / (LAST_RUSH_TIME - FIRST_RUSH_TIME);
            _marker.transform.localPosition = Vector3.LerpUnclamped(first, last, t);

            // Same window rushes use: animate just before the boss is due, and briefly after they arrive.
            bool triggered = _state.Arrived
                ? _timeOfDay < _state.ArrivedAt + 0.1f
                : _timeOfDay > _state.ArrivalTime - 0.05f;
            _marker.SetBool(IsTriggered, triggered);
        }

        private static Animator CreateMarker(TimeDisplayView display)
        {
            List<Animator> rushMarkers = RushMarkers(display);
            if (rushMarkers == null || rushMarkers.Count < 3 || rushMarkers[0] == null || rushMarkers[1] == null || rushMarkers[2] == null)
            {
                Mod.Logger.LogWarning("Timeline rush markers not found, boss marker disabled");
                return null;
            }

            Animator source = rushMarkers[1];
            GameObject clone = Object.Instantiate(source.gameObject, source.transform.parent);
            clone.name = "BossUp Boss Marker";

            TMP_Text label = clone.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = BOSS_ICON;

            return clone.GetComponent<Animator>();
        }
    }
}
