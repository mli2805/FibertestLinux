using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class NetworkEventsViewModel : PropertyChangedBase
    {
        public string TableTitle { get; set; } = null!;

        private readonly Model _readModel;
      
        private ObservableCollection<NetworkEventModel> _rows = new ObservableCollection<NetworkEventModel>();
        public ObservableCollection<NetworkEventModel> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        public NetworkEventsViewModel(Model readModel)
        {
            _readModel = readModel;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"EventTimestamp", ListSortDirection.Descending));
        }

        public void AddEvent(NetworkEvent networkEvent)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == networkEvent.RtuId);
            Rows.Add(new NetworkEventModel()
            {
                Ordinal = networkEvent.Ordinal,
                EventTimestamp = networkEvent.EventTimestamp,
                RtuId = networkEvent.RtuId,
                RtuTitle = rtu.Title,
                IsRtuAvailable = networkEvent.IsRtuAvailable,
                OnMainChannel = networkEvent.OnMainChannel,
                OnReserveChannel = networkEvent.OnReserveChannel,
                MainChannel = networkEvent.OnMainChannel == ChannelEvent.Nothing 
                    ? rtu.MainChannelState
                    : networkEvent.OnMainChannel == ChannelEvent.Broken 
                        ? RtuPartState.Broken 
                        : RtuPartState.Ok,
                ReserveChannel = networkEvent.OnReserveChannel == ChannelEvent.Nothing
                    ? rtu.ReserveChannelState
                    : networkEvent.OnReserveChannel == ChannelEvent.Broken
                        ? RtuPartState.Broken
                        : RtuPartState.Ok,
            });
        }

        public void RemoveOldEventForRtuIfExists(Guid rtuId)
        {
            var oldEvent = Rows.FirstOrDefault(r => r.RtuId == rtuId);
            if (oldEvent != null)
                Rows.Remove(oldEvent);
        }

        public void RemoveAllEventsForRtu(Guid rtuId)
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].RtuId == rtuId)
                    Rows.RemoveAt(i);
            }
        }

        public void RefreshRowsWithUpdatedRtu(Guid rtuId)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return;
            foreach (var networkEventModel in Rows.Where(m => m.RtuId == rtuId).ToList())
            {
                Rows.Remove(networkEventModel);
                networkEventModel.RtuTitle = rtu.Title;
                Rows.Add(networkEventModel);
            }
        }

        public void RemoveEventsAndSors(EventsAndSorsRemoved evnt)
        {
            if (!evnt.IsNetworkEvents) return;

            foreach (var networkEventModel in Rows.ToList())
            {
                if (_readModel.NetworkEvents.All(n=>n.EventTimestamp != networkEventModel.EventTimestamp))
                    Rows.Remove(networkEventModel);
            }
        }
      
    }
}
