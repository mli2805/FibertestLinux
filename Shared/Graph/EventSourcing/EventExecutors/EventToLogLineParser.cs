using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.Graph
{
    public class EventToLogLineParser
    {
        private readonly Model _readModel;

        // rtuId - Title
        private Dictionary<Guid, string> _rtuTitles;

        // traceId - <Title, rtuId>
        private Dictionary<Guid, Tuple<string, Guid>> _traces;

        // tceId - Title
        private Dictionary<Guid, string> _tceTitles;

        // SorfileId - Measurement
        private Dictionary<int, MeasurementAdded> _measurements;

        public EventToLogLineParser(Model readModel)
        {
            _readModel = readModel;
            _rtuTitles = new Dictionary<Guid, string>();
            _traces = new Dictionary<Guid, Tuple<string, Guid>>();
            _tceTitles = new Dictionary<Guid, string>();
            _measurements = new Dictionary<int, MeasurementAdded>();
        }

        public LogLine? ParseEventBody(object body)
        {
            switch (body)
            {
                case RtuAtGpsLocationAdded evnt: return Parse(evnt);
                case RtuUpdated evnt: return Parse(evnt);
                case RtuInitialized evnt: return Parse(evnt);
                case RtuRemoved evnt: return Parse(evnt);

                case TraceAdded evnt: return Parse(evnt);
                case TraceUpdated evnt: return Parse(evnt);
                case TraceAttached evnt: return Parse(evnt);
                case TraceDetached evnt: return Parse(evnt);
                case TraceCleaned evnt: return Parse(evnt);
                case TraceRemoved evnt: return Parse(evnt);
                case BaseRefAssigned evnt: return Parse(evnt);

                case TceWithRelationsAddedOrUpdated evnt: return Parse(evnt);
                case TceRemoved evnt: return Parse(evnt);

                case MonitoringSettingsChanged evnt: return Parse(evnt);
                case MonitoringStarted evnt: return Parse(evnt);
                case MonitoringStopped evnt: return Parse(evnt);

                case ClientStationRegistered evnt: return Parse(evnt);
                case ClientStationUnregistered _: return new LogLine() { OperationCode = LogOperationCode.ClientExited };
                case ClientConnectionLost _:
                    return new LogLine() { OperationCode = LogOperationCode.ClientConnectionLost };
                case UsersMachineKeyAssigned evnt: return Parse(evnt);

                case MeasurementAdded evnt: return Parse(evnt);
                case MeasurementUpdated evnt: return Parse(evnt);

                case EventsAndSorsRemoved evnt: return Parse(evnt);
                case SnapshotMade evnt: return Parse(evnt);
                default: return null;
            }
        }

        public void InitializeBySnapshot(Model modelAtSnapshot)
        {
            foreach (var rtu in modelAtSnapshot.Rtus)
                _rtuTitles.Add(rtu.Id, rtu.Title);

            foreach (var trace in modelAtSnapshot.Traces)
                _traces.Add(trace.TraceId, new Tuple<string, Guid>(trace.Title, trace.RtuId));

            foreach (var tce in modelAtSnapshot.TcesNew)
                _tceTitles.Add(tce.Id, tce.Title);

        }

        private LogLine Parse(RtuAtGpsLocationAdded e)
        {
            _rtuTitles.Add(e.Id, e.Title);
            return new LogLine() { OperationCode = LogOperationCode.RtuAdded, RtuTitle = e.Title };
        }

        private LogLine Parse(RtuUpdated e)
        {
            _rtuTitles[e.RtuId] = e.Title;
            return new LogLine { OperationCode = LogOperationCode.RtuUpdated, RtuTitle = e.Title };
        }

        private LogLine Parse(RtuInitialized e)
        {
            return new LogLine { OperationCode = LogOperationCode.RtuInitialized, RtuTitle = _rtuTitles[e.Id] };
        }

        private LogLine Parse(RtuRemoved e)
        {
            return new LogLine { OperationCode = LogOperationCode.RtuRemoved, RtuTitle = _rtuTitles[e.RtuId] };
        }

        private LogLine Parse(TraceAdded e)
        {
            _traces.Add(e.TraceId, new Tuple<string, Guid>(e.Title, e.RtuId));
            return new LogLine()
            {
                OperationCode = LogOperationCode.TraceAdded,
                RtuTitle = _rtuTitles[e.RtuId],
                TraceTitle = e.Title
            };
        }

        private LogLine Parse(TraceUpdated e)
        {
            var rtuId = _traces[e.Id].Item2;
            _traces[e.Id] = new Tuple<string, Guid>(e.Title, rtuId);
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceUpdated,
                RtuTitle = _rtuTitles[_traces[e.Id].Item2],
                TraceTitle = e.Title,
            };
        }

        private LogLine Parse(TraceAttached e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceAttached,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
                OperationParams = $@"port {e.OtauPortDto.OpticalPort}",
            };
        }

        private LogLine Parse(TraceDetached e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceDetached,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        private LogLine Parse(TraceCleaned e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceCleaned,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        private LogLine Parse(TraceRemoved e)
        {
            return new LogLine
            {
                OperationCode = LogOperationCode.TraceRemoved,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
            };
        }

        private LogLine Parse(BaseRefAssigned e)
        {
            var additionalInfo = "";
            foreach (var baseRef in e.BaseRefs)
            {
                additionalInfo = additionalInfo + baseRef.BaseRefType.GetLocalizedFemaleString() + @"; ";
            }

            return new LogLine()
            {
                OperationCode = LogOperationCode.BaseRefAssigned,
                RtuTitle = _rtuTitles[_traces[e.TraceId].Item2],
                TraceTitle = _traces[e.TraceId].Item1,
                OperationParams = additionalInfo,
            };
        }

        private LogLine Parse(TceWithRelationsAddedOrUpdated e)
        {
            var isCreation = false;
            if (!_tceTitles.ContainsKey(e.Id))
            {
                isCreation = true;
                _tceTitles.Add(e.Id, e.Title);
            }

            var additionalInfo = isCreation
                ? $@"TCE {e.Title}, {e.AllRelationsOfTce.Count} " + Resources.SID_links_added
                : $@"TCE {e.Title}, {e.ExcludedTraceIds.Count} " + Resources.SID_links_removed + $@", {e.AllRelationsOfTce.Count} " + Resources.SID_links_exist;
            return new LogLine()
            {
                OperationCode = isCreation ? LogOperationCode.TceAdded : LogOperationCode.TceUpdated,
                OperationParams = additionalInfo,
            };
        }

        private LogLine Parse(TceRemoved e)
        {
            return new LogLine()
            {
                OperationCode = LogOperationCode.TceRemoved,
                OperationParams = $@"TCE {_tceTitles[e.Id]}",
            };
        }

        private LogLine Parse(MonitoringSettingsChanged e)
        {
            var mode = e.IsMonitoringOn ? @"AUTO" : @"MANUAL";
            return new LogLine()
            {
                OperationCode = LogOperationCode.MonitoringSettingsChanged,
                RtuTitle = _rtuTitles[e.RtuId],
                OperationParams = $@"Mode - {mode}",
            };
        }

        private LogLine Parse(MonitoringStarted e)
        {
            return new LogLine() { OperationCode = LogOperationCode.MonitoringStarted, RtuTitle = _rtuTitles[e.RtuId] };
        }

        private LogLine Parse(MonitoringStopped e)
        {
            return new LogLine() { OperationCode = LogOperationCode.MonitoringStopped, RtuTitle = _rtuTitles[e.RtuId] };
        }

        private LogLine Parse(ClientStationRegistered e)
        {
            return new LogLine()
            {
                OperationCode = LogOperationCode.ClientStarted,
                OperationParams = e.RegistrationResult.GetLocalizedString(),
            };
        }

        private LogLine Parse(UsersMachineKeyAssigned e)
        {
            return new LogLine()
            {
                OperationCode = LogOperationCode.UsersMachineKeyAssigned,
                OperationParams = _readModel.Users.FirstOrDefault(u => u.UserId == e.UserId)?.Title ?? ""
            };
        }

        private LogLine? Parse(MeasurementAdded e)
        {
            _measurements.Add(e.SorFileId, e);
            return null;
        }

        private LogLine Parse(MeasurementUpdated e)
        {
            var meas = _measurements[e.SorFileId];
            return new LogLine()
            {
                OperationCode = LogOperationCode.MeasurementUpdated,
                RtuTitle = _rtuTitles[meas.RtuId],
                TraceTitle = _traces[meas.TraceId].Item1,
                OperationParams = $@"{e.EventStatus.GetLocalizedString()}",
            };
        }


        private LogLine Parse(EventsAndSorsRemoved e)
        {
            var str = Resources.SID_Up_to;
            return new LogLine()
            {
                OperationCode = LogOperationCode.EventsAndSorsRemoved,
                OperationParams = $@"{str} {e.UpTo.Date:d}  {(e.IsMeasurementsNotEvents ? 1 : 0)}/{(e.IsOpticalEvents ? 1 : 0)}/{(e.IsNetworkEvents ? 1 : 0)}",
            };
        }

        private LogLine Parse(SnapshotMade e)
        {
            var str = Resources.SID_Up_to;
            return new LogLine()
            {
                OperationCode = LogOperationCode.SnapshotMade,
                OperationParams = $@"{str} {e.UpTo.Date:d}",
            };
        }
    }
}