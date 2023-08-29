using Fibertest.Dto;

namespace Fibertest.DataCenter
{
    public partial class Web2DCommandsProcessor
    {
        public async Task<OpticalEventsRequestedDto> GetOpticalEventPortion(
            string username, bool isCurrentEvents, string filterRtu = "", string filterTrace = "", 
            string sortOrder = "desc", int pageNumber = 0, int pageSize = 100)
        {
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            var collection = isCurrentEvents ? _writeModel.ActiveMeasurements : _writeModel.Measurements;
            var sift = collection
                .Where(o => o.Filter(filterRtu, filterTrace, _writeModel, user)).ToList();
            return new OpticalEventsRequestedDto
            {
                FullCount = sift.Count,
                EventPortion = sift
                    .Sort(sortOrder)
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(m => m.CreateOpticalEventDto(_writeModel)).ToList()
            };
        }

        private async Task<List<OpticalAlarm>> GetCurrentOpticalEvents(string username)
        {
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            var sift = _writeModel.ActiveMeasurements.Where(o => o.Filter(null, null, _writeModel, user)).ToList();
            return sift.Select(m => m.CreateOpticalAlarm()).ToList();
        }

        public async Task<NetworkEventsRequestedDto> GetNetworkEventPortion(string username, bool isCurrentEvents,
            string filterRtu, string sortOrder, int pageNumber, int pageSize)
        {
            var user = _writeModel.Users.First(u => u.Title == username);
            var sift = isCurrentEvents
                ? _writeModel.Rtus
                    .Where(r => r.FilterRtuWithProblems(user, filterRtu))
                    .Select(rtu => _writeModel.NetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id))
                    .Where(lastNetworkEvent => lastNetworkEvent != null).ToList()
                : _writeModel.NetworkEvents
                    .Where(n => n.Filter(filterRtu, _writeModel, user)).ToList();
            return new NetworkEventsRequestedDto
            {
                FullCount = sift.Count,
                EventPortion = sift
                    .Sort(sortOrder)
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(m => m.CreateNetworkEventDto(_writeModel)).ToList()
            };
        }

        private async Task<List<NetworkAlarm>> GetCurrentNetworkEvents(string username)
        {
            var user = _writeModel.Users.First(u => u.Title == username);
            var sift = _writeModel.Rtus
                .Where(r => r.FilterRtuWithProblems(user, null))
                .Select(rtu => _writeModel.NetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id))
                .Where(lastNetworkEvent => lastNetworkEvent != null).ToList();
            var result = new List<NetworkAlarm>();
            foreach (var networkEvent in sift)
                result.AddRange(networkEvent.CreateNetworkAlarms());
            return result;
        }

        public async Task<BopEventsRequestedDto> GetBopEventPortion(string username, bool isCurrentEvents,
            string filterRtu, string sortOrder, int pageNumber, int pageSize)
        {
            var user = _writeModel.Users.First(u => u.Title == username);
            var sift = isCurrentEvents
                ? _writeModel.Rtus
                    .Where(r => r.FilterRtu(user, filterRtu))
                    .Select(rtu => _writeModel.BopNetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id))
                    .Where(lastBopNetworkEvent => lastBopNetworkEvent != null && !lastBopNetworkEvent.IsOk).ToList()
                : _writeModel.BopNetworkEvents
                    .Where(n => n.Filter(filterRtu, _writeModel, user)).ToList();
            return new BopEventsRequestedDto
            {
                FullCount = sift.Count,
                EventPortion = sift
                    .Sort(sortOrder)
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(m => m.CreateBopEventDto(_writeModel)).ToList()
            };
        }

        private async Task<List<BopAlarm>> GetCurrentBopEvents(string username)
        {
            var user = _writeModel.Users.First(u => u.Title == username);
            return _writeModel.Rtus
                .Where(r => r.FilterRtu(user, null))
                .Select(rtu => _writeModel.BopNetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id))
                .Where(lastBopNetworkEvent => lastBopNetworkEvent != null && !lastBopNetworkEvent.IsOk)
                .Select(b => b.CreateBopAlarm())
                .ToList();
        }
    }
}
