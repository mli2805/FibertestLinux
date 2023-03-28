using System;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class NetworkEventsDoubleViewModel : PropertyChangedBase
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly SystemState _systemState;

        public NetworkEventsViewModel ActualNetworkEventsViewModel { get; set; }
        public NetworkEventsViewModel AllNetworkEventsViewModel { get; set; }

        public NetworkEventsDoubleViewModel(Model readModel, CurrentUser currentUser, SystemState systemState,
            NetworkEventsViewModel actualNetworkEventsViewModel, NetworkEventsViewModel allNetworkEventsViewModel)
        {
            ActualNetworkEventsViewModel = actualNetworkEventsViewModel;
            ActualNetworkEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllNetworkEventsViewModel = allNetworkEventsViewModel;
            AllNetworkEventsViewModel.TableTitle = Resources.SID_All_network_events;
            _readModel = readModel;
            _currentUser = currentUser;
            _systemState = systemState;
        }

        public void Apply(object evnt)
        {
            switch (evnt)
            {
                case NetworkEventAdded e: RtuAvailabilityChanged(e); break;
                case RtuUpdated e: RtuUpdated(e.RtuId); break;
                case RtuRemoved e: RtuRemoved(e.RtuId); break;
                case ResponsibilitiesChanged e: ChangeResponsibilities(e); break;
                case EventsAndSorsRemoved e: AllNetworkEventsViewModel.RemoveEventsAndSors(e); break;
                default: return;
            }

            _systemState.HasActualNetworkProblems = ActualNetworkEventsViewModel.Rows.Any();
        }

        private void RtuAvailabilityChanged(NetworkEventAdded networkEventAdded)
        {
            var networkEvent = Mapper.Map<NetworkEvent>(networkEventAdded);
            ApplyOneEvent(networkEvent);
        }

        public void RenderNetworkEvents()
        {
            foreach (var networkEvent in _readModel.NetworkEvents)
            {
               ApplyOneEvent(networkEvent);
            }
        }

        private void ApplyOneEvent(NetworkEvent networkEvent)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == networkEvent.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId))
                return;

            AllNetworkEventsViewModel.AddEvent(networkEvent);
            ActualNetworkEventsViewModel.RemoveOldEventForRtuIfExists(networkEvent.RtuId);

            if (rtu.IsAllRight)
                return;

            ActualNetworkEventsViewModel.AddEvent(networkEvent);
        }

        private void RtuUpdated(Guid rtuId)
        {
            ActualNetworkEventsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
            AllNetworkEventsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
        }

        private void RtuRemoved(Guid rtuId)
        {
            ActualNetworkEventsViewModel.RemoveAllEventsForRtu(rtuId);
            AllNetworkEventsViewModel.RemoveAllEventsForRtu(rtuId);
        }

        private void ChangeResponsibilities(ResponsibilitiesChanged evnt)
        {
            foreach (var pair in evnt.ResponsibilitiesDictionary)
            {
                var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == pair.Key);
                if (rtu == null) continue; // not interested here in traces
                if (!pair.Value.Contains(_currentUser.ZoneId)) continue; // for current zone this RTU doesn't change

                if (rtu.ZoneIds.Contains(_currentUser.ZoneId)) // was NOT became YES
                {
                    var lastNetworkEvent = _readModel.NetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id);
                    if (lastNetworkEvent != null && !rtu.IsAllRight)
                        ActualNetworkEventsViewModel.AddEvent(lastNetworkEvent);

                    foreach (var networkEvent in _readModel.NetworkEvents.Where(n => n.RtuId == rtu.Id))
                    {
                        AllNetworkEventsViewModel.AddEvent(networkEvent);
                    }
                }
                else
                {
                   RtuRemoved(rtu.Id);
                }
            }
        }

    }
}
