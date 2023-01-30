using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class BopNetworkEventsDoubleViewModel : PropertyChangedBase
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly SystemState _systemState;

        public BopNetworkEventsViewModel ActualBopNetworkEventsViewModel { get; set; }
        public BopNetworkEventsViewModel AllBopNetworkEventsViewModel { get; set; }

        public BopNetworkEventsDoubleViewModel(Model readModel, CurrentUser currentUser, SystemState systemState,
            BopNetworkEventsViewModel actualBopNetworkEventsViewModel, BopNetworkEventsViewModel allBopNetworkEventsViewModel)
        {
            ActualBopNetworkEventsViewModel = actualBopNetworkEventsViewModel;
            ActualBopNetworkEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllBopNetworkEventsViewModel = allBopNetworkEventsViewModel;
            AllBopNetworkEventsViewModel.TableTitle = Resources.SID_All_BOP_network_events;
            _readModel = readModel;
            _currentUser = currentUser;
            _systemState = systemState;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case BopNetworkEventAdded evnt: AddBopNetworkEvent(evnt); break;
                case OtauDetached evnt: DetachOtau(evnt); break;
                case EventsAndSorsRemoved evnt: AllBopNetworkEventsViewModel.RemoveEventsAndSors(evnt); break;
            }

            _systemState.HasActualBopNetworkProblems = ActualBopNetworkEventsViewModel.Rows.Any();
        }

        private void AddBopNetworkEvent(BopNetworkEventAdded evnt1)
        {
            var evnt = Mapper.Map<BopNetworkEvent>(evnt1);
            ApplyOneEvent(evnt);
        }

        public void RenderBopNetworkEvents()
        {
            foreach (var bopNetworkEvent in _readModel.BopNetworkEvents)
            {
                ApplyOneEvent(bopNetworkEvent);
            }
        }

        private void ApplyOneEvent(BopNetworkEvent evnt)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == evnt.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId))
                return;

            var bop = _readModel.Otaus.FirstOrDefault(o => o.NetAddress.Ip4Address == evnt.OtauIp && o.NetAddress.Port == evnt.TcpPort);
            if (bop == null)
            {
                return;
            }

            if (bop.NetAddress == null)
            {
                evnt.OtauIp = "";
                evnt.TcpPort = -1;
            }
            else
            {
                evnt.OtauIp =  bop.NetAddress.Ip4Address;
                evnt.TcpPort = bop.NetAddress.Port;
            }
          

            AllBopNetworkEventsViewModel.AddEvent(evnt);
            ActualBopNetworkEventsViewModel.RemoveOldEventForBopIfExists(evnt.OtauIp);

            if (evnt.IsOk)
                return;

            ActualBopNetworkEventsViewModel.AddEvent(evnt);
        }

        private void DetachOtau(OtauDetached evnt)
        {
            ActualBopNetworkEventsViewModel.RemoveEvents(evnt);
            AllBopNetworkEventsViewModel.RemoveEvents(evnt);
        }

    }
}
