using BossUp.Boss;
using Kitchen;
using KitchenLib.Utils;
using KitchenMods;
using MessagePack;
using Unity.Collections;
using Unity.Entities;

namespace BossUp.Views
{
    /// <summary>Tags the entity that owns the boss timeline view. Registered with KitchenLib's ViewCreator.</summary>
    public struct SBossTimelineView : IComponentData
    {
    }

    /// <summary>
    /// Carries today's boss plan from the host to every player, so each screen can draw the boss
    /// marker on its day timeline. The view itself draws nothing; it hands the data to
    /// <see cref="BossTimelineMarker"/>, which adds the marker to the game's own timeline.
    /// </summary>
    public class BossTimelineView : UpdatableObjectView<BossTimelineView.ViewData>
    {
        public const string VIEW_TYPE_NAME = "BossUp.Views.BossTimelineView";
        public static readonly ViewType ViewType = (ViewType)VariousUtils.GetID(VIEW_TYPE_NAME);

        /// <summary>Host side: sends the boss plan whenever it changes.</summary>
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery _views;

            protected override void Initialise()
            {
                base.Initialise();
                _views = GetEntityQuery(typeof(SBossTimelineView), typeof(CLinkedView));
            }

            protected override void OnUpdate()
            {
                var data = new ViewData
                {
                    Show = BossToday.IsBossDay && BossSettings.Enabled && BossSettings.TimelineMarker,
                    ArrivalTime = BossToday.ArrivalTime,
                    Arrived = BossToday.Arrived,
                    ArrivedAt = BossToday.ArrivedAt
                };

                using (NativeArray<CLinkedView> views = _views.ToComponentDataArray<CLinkedView>(Allocator.Temp))
                {
                    foreach (CLinkedView view in views)
                        SendUpdate(view.Identifier, data);
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : IViewData, IViewResponseData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool Show;
            [Key(1)] public float ArrivalTime;
            [Key(2)] public bool Arrived;
            [Key(3)] public float ArrivedAt;

            public bool IsChangedFrom(ViewData check)
            {
                return Show != check.Show || ArrivalTime != check.ArrivalTime || Arrived != check.Arrived || ArrivedAt != check.ArrivedAt;
            }
        }

        protected override void UpdateData(ViewData data)
        {
            BossTimelineMarker.SetState(data);
        }

        public override void Remove()
        {
            BossTimelineMarker.SetState(default);
            base.Remove();
        }
    }
}
