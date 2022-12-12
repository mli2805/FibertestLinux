using AutoMapper;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace Fibertest.Graph
{
    public class CommandAggregator
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();

        private readonly Model _writeModel;
        private readonly ILogger<CommandAggregator> _logger;
        private readonly EventsQueue _eventsQueue;

        //TODO. Should be initialized depending on where it is called from (DataCenter or Client)
        private readonly int _logF = Logs.DataCenter.ToInt();

        public CommandAggregator(Model writeModel, ILogger<CommandAggregator> logger, EventsQueue eventsQueue)
        {
            _writeModel = writeModel;
            _logger = logger;
            _eventsQueue = eventsQueue;
        }

        public string? Validate(object cmd)
        {
            switch (cmd)
            {
                case RegisterClientStation command: return _eventsQueue.Add(Mapper.Map<ClientStationRegistered>(command));
                case UnregisterClientStation _: return _eventsQueue.Add(new ClientStationUnregistered());
                case LostClientConnection _: return _eventsQueue.Add(new ClientConnectionLost());

                case AddUser command: return _eventsQueue.Add(Mapper.Map<UserAdded>(command));
                case UpdateUser command: return _eventsQueue.Add(Mapper.Map<UserUpdated>(command));
                case AssignUsersMachineKey command: return _eventsQueue.Add(Mapper.Map<UsersMachineKeyAssigned>(command));
                case RemoveUser command: return _eventsQueue.Add(Mapper.Map<UserRemoved>(command));

                case AddZone command: return _eventsQueue.Add(Mapper.Map<ZoneAdded>(command));
                case UpdateZone command: return _eventsQueue.Add(Mapper.Map<ZoneUpdated>(command));
                case RemoveZone command: return _eventsQueue.Add(Mapper.Map<ZoneRemoved>(command));

                case ChangeResponsibilities command: return _eventsQueue.Add(Mapper.Map<ResponsibilitiesChanged>(command));

                case AddNodeIntoFiber command: return _eventsQueue.Add(Mapper.Map<NodeIntoFiberAdded>(command));
                case UpdateNode command: return _eventsQueue.Add(Mapper.Map<NodeUpdated>(command));
                case UpdateAndMoveNode command: return _eventsQueue.Add(Mapper.Map<NodeUpdatedAndMoved>(command));
                case MoveNode command: return _eventsQueue.Add(Mapper.Map<NodeMoved>(command));
                case RemoveNode command: return Validate(command);

                case AddFiber command: return Validate(command);
                case AddFiberWithNodes command: return Validate(command);
                case UpdateFiber command: return _eventsQueue.Add(Mapper.Map<FiberUpdated>(command));
                case RemoveFiber command: return Validate(command);

                case RemoveUnused command: return _eventsQueue.Add(Mapper.Map<UnusedRemoved>(command));

                case AddEquipmentIntoNode command: return Validate(command);
                case AddEquipmentAtGpsLocation command: return _eventsQueue.Add(Mapper.Map<EquipmentAtGpsLocationAdded>(command));
                case AddEquipmentAtGpsLocationWithNodeTitle command: return _eventsQueue.Add(Mapper.Map<EquipmentAtGpsLocationWithNodeTitleAdded>(command));
                case UpdateEquipment command: return _eventsQueue.Add(Mapper.Map<EquipmentUpdated>(command));
                case RemoveEquipment command: return _eventsQueue.Add(Mapper.Map<EquipmentRemoved>(command));
                case IncludeEquipmentIntoTrace command: return _eventsQueue.Add(Mapper.Map<EquipmentIntoTraceIncluded>(command));
                case ExcludeEquipmentFromTrace command: return _eventsQueue.Add(Mapper.Map<EquipmentFromTraceExcluded>(command));

                case AddRtuAtGpsLocation command: return _eventsQueue.Add(Mapper.Map<RtuAtGpsLocationAdded>(command));
                case UpdateRtu command: return _eventsQueue.Add(Mapper.Map<RtuUpdated>(command));
                case RemoveRtu command: return Validate(command);
                case AttachOtau command: return _eventsQueue.Add(Mapper.Map<OtauAttached>(command));
                case DetachOtau command: return _eventsQueue.Add(Mapper.Map<OtauDetached>(command));
                case DetachAllTraces command: return _eventsQueue.Add(Mapper.Map<AllTracesDetached>(command));

                case AddTrace command: return Validate(command);
                case UpdateTrace command: return _eventsQueue.Add(Mapper.Map<TraceUpdated>(command));
                case CleanTrace command: return Validate(command);
                case RemoveTrace command: return Validate(command);
                case AttachTrace command: return Complete(command);
                case DetachTrace command: return _eventsQueue.Add(Mapper.Map<TraceDetached>(command));

                case AddOrUpdateTceWithRelations command: return _eventsQueue.Add(Mapper.Map<TceWithRelationsAddedOrUpdated>(command));
                case RemoveTce command: return _eventsQueue.Add(Mapper.Map<TceRemoved>(command));
                case ReSeedTceTypeStructList command: return _eventsQueue.Add(Mapper.Map<TceTypeStructListReSeeded>(command));

                case AddVeexTest command: return _eventsQueue.Add(Mapper.Map<VeexTestAdded>(command));
                case RemoveVeexTest command: return _eventsQueue.Add(Mapper.Map<VeexTestRemoved>(command));

                case AssignBaseRef command: return _eventsQueue.Add(Mapper.Map<BaseRefAssigned>(command));
                case ChangeMonitoringSettings command: return _eventsQueue.Add(Mapper.Map<MonitoringSettingsChanged>(command));
                case InitializeRtu command: return _eventsQueue.Add(Mapper.Map<RtuInitialized>(command));
                case StartMonitoring command: return _eventsQueue.Add(Mapper.Map<MonitoringStarted>(command));
                case StopMonitoring command: return _eventsQueue.Add(Mapper.Map<MonitoringStopped>(command));
                case AddMeasurement command: return Validate(command);
                case UpdateMeasurement command: return _eventsQueue.Add(Mapper.Map<MeasurementUpdated>(command));
                case ApplyLicense command: return Validate(command);
                case AddNetworkEvent command: return Validate(command);
                case AddBopNetworkEvent command: return Validate(command);

                case RemoveEventsAndSors command: return _eventsQueue.Add(Mapper.Map<EventsAndSorsRemoved>(command));
                case MakeSnapshot command: return _eventsQueue.Add(Mapper.Map<SnapshotMade>(command));

                default: return @"Unknown command";
            }
        }

        private string? Complete(AttachTrace cmd)
        {
            var evnt = Mapper.Map<TraceAttached>(cmd);
            var measurement = _writeModel.Measurements.LastOrDefault(m => m.TraceId == cmd.TraceId);
            if (measurement != null)
            {
                evnt.PreviousTraceState = measurement.TraceState;
                if (measurement.TraceState != FiberState.Ok &&
                    measurement.TraceState != FiberState.NoFiber)
                    evnt.AccidentsInLastMeasurement = measurement.Accidents;
            }
            else
            { // trace has no measurements so far 
                evnt.PreviousTraceState = FiberState.Unknown;
            }
            return _eventsQueue.Add(evnt);
        }

        private string? Validate(ApplyLicense cmd)
        {
            if (cmd.IsIncremental)
            {
                if (!_writeModel.Licenses.Any())
                    return ((int)ReturnCode.FirstLicenseKeyMustNotBeIncremental).ToString();
                if (cmd.WebClientCount.Value > 0 &&
                    _writeModel.Licenses.First(l => !l.IsIncremental).IsMachineKeyRequired)
                    return ((int)ReturnCode.LicenseCouldNotBeApplied).ToString();
            }
            if (_writeModel.Licenses.Any(l => l.LicenseId == cmd.LicenseId))
                return ((int)ReturnCode.LicenseCouldNotBeAppliedRepeatedly).ToString();
            return _eventsQueue.Add(Mapper.Map<LicenseApplied>(cmd));
        }

        private string? Validate(RemoveNode cmd)
        {
            if (_writeModel.Traces.Any(t => t.NodeIds.Last() == cmd.NodeId))
                return Resources.SID_It_s_prohibited_to_remove_last_node_from_trace;
            if (_writeModel.Traces.Any(t => t.HasAnyBaseRef && t.NodeIds.Contains(cmd.NodeId) && !cmd.IsAdjustmentPoint))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            var evnt = Mapper.Map<NodeRemoved>(cmd); // mapper copies dictionary and list successfully
            return _eventsQueue.Add(evnt);
        }

        #region Fiber
        private string? Validate(AddFiber cmd)
        {
            // for Kadastr Loader
            if (_writeModel.Nodes.FirstOrDefault(n => n.NodeId == cmd.NodeId1) == null
                || _writeModel.Nodes.FirstOrDefault(n => n.NodeId == cmd.NodeId2) == null)
                return @"At least one node does not exist";

            if (_writeModel.HasDirectFiberDontMindPoints(cmd.NodeId1, cmd.NodeId2))
                return Resources.SID_Section_already_exists;

            return _eventsQueue.Add(Mapper.Map<FiberAdded>(cmd));
        }

        private string? Validate(AddFiberWithNodes cmd)
        {
            Guid a = cmd.Node1;
            Guid b = cmd.Node2;
            if (_writeModel.Fibers.Any(f =>
                f.NodeId1 == a && f.NodeId2 == b ||
                f.NodeId1 == b && f.NodeId2 == a))
                return Resources.SID_Section_already_exists;


            foreach (var cmdAddEquipmentAtGpsLocation in cmd.AddEquipments)
            {
                var result = _eventsQueue.Add(Mapper.Map<EquipmentAtGpsLocationAdded>(cmdAddEquipmentAtGpsLocation));
                if (result != null)
                    return result;
            }

            foreach (var cmdAddFiber in cmd.AddFibers)
            {
                var result = _eventsQueue.Add(Mapper.Map<FiberAdded>(cmdAddFiber));
                if (result != null)
                    return result;
            }

            return null;
        }

        private string? Validate(RemoveFiber cmd)
        {
            if (IsFiberContainedInAnyTraceWithBase(cmd.FiberId))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;
            return _eventsQueue.Add(Mapper.Map<FiberRemoved>(cmd));
        }

        private bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = _writeModel.Traces.Where(t => t.HasAnyBaseRef);
            var fiber = _writeModel.Fibers.First(f => f.FiberId == fiberId);
            return tracesWithBase.Any(trace => trace.FiberIds.IndexOf(fiber.FiberId) != -1);
        }
        #endregion

        private string? Validate(AddEquipmentIntoNode cmd)
        {
            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
                if (trace == null)
                {
                    var message = $@"AddEquipmentIntoNode: Trace {traceId.First6()} not found";
                    _logger.Log(LogLevel.Error, _logF, message);
                    return message;
                }
                if (trace.HasAnyBaseRef)
                    return Resources.SID_Base_ref_is_set_for_trace;
            }
            var result = _eventsQueue.Add(Mapper.Map<EquipmentIntoNodeAdded>(cmd));
            if (result != null)
                return result;

            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
                if (trace == null)
                {
                    var message = $@"AddEquipmentIntoNode: Trace {traceId.First6()} not found";
                    _logger.Log(LogLevel.Error, _logF, message);
                    return message;
                }
                var idx = trace.NodeIds.IndexOf(cmd.NodeId);
                trace.EquipmentIds[idx] = cmd.EquipmentId;
            }
            return null;
        }

        private string? Validate(RemoveRtu cmd)
        {
            var evnt = Mapper.Map<RtuRemoved>(cmd);
            evnt.FibersFromCleanedTraces = new List<KeyValuePair<Guid, Guid>>();
            foreach (var trace in _writeModel.Traces.Where(t => t.RtuId == cmd.RtuId))
            {
                foreach (var fiberId in trace.FiberIds)
                {
                    evnt.FibersFromCleanedTraces.Add(new KeyValuePair<Guid, Guid>(fiberId, trace.TraceId));
                }
            }
            return _eventsQueue.Add(evnt);
        }

        #region Trace
        private string? Validate(AddTrace cmd)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == cmd.RtuId);
            if (rtu == null)
                return Resources.SID_RTU_is_not_found;
            if (cmd.EquipmentIds[0] != cmd.RtuId)
                return Resources.SID_Trace_should_start_from_RTU;
            if (cmd.NodeIds.Count != cmd.EquipmentIds.Count)
                return Resources.SID_Equipments_count_in_trace_should_match_nodes_count;
            if (cmd.EquipmentIds.Last() == Guid.Empty)
                return Resources.SID_Last_node_of_trace_must_contain_some_equipment;

            return _eventsQueue.Add(Mapper.Map<TraceAdded>(cmd));
        }

        private string? Validate(CleanTrace cmd)
        {
            var traceCleaned = Mapper.Map<TraceCleaned>(cmd);
            var trace = _writeModel.Traces.First(t => t.TraceId == cmd.TraceId);
            traceCleaned.NodeIds = trace.NodeIds;
            traceCleaned.FiberIds = trace.FiberIds;
            return _eventsQueue.Add(traceCleaned);
        }

        private string? Validate(RemoveTrace cmd)
        {
            var traceRemoved = Mapper.Map<TraceRemoved>(cmd);
            var trace = _writeModel.Traces.First(t => t.TraceId == cmd.TraceId);
            traceRemoved.NodeIds = trace.NodeIds;
            traceRemoved.FiberIds = trace.FiberIds;
            return _eventsQueue.Add(traceRemoved);
        }
        #endregion

        private string? Validate(AddMeasurement cmd)
        {
            if (_writeModel.Traces.All(t => t.TraceId != cmd.TraceId))
                return $@"Unknown trace {cmd.TraceId.First6()}";
            return _eventsQueue.Add(Mapper.Map<MeasurementAdded>(cmd));
        }

        private string? Validate(AddNetworkEvent cmd)
        {
            var networkEventAdded = Mapper.Map<NetworkEventAdded>(cmd);
            var lastEventOrdinal = _writeModel.NetworkEvents.Any() ? _writeModel.NetworkEvents.Max(n => n.Ordinal) : 1;
            networkEventAdded.Ordinal = lastEventOrdinal + 1;
            return _eventsQueue.Add(networkEventAdded);
        }

        private string? Validate(AddBopNetworkEvent cmd)
        {
            var otau = _writeModel.Otaus.FirstOrDefault(o => o.NetAddress.Ip4Address == cmd.OtauIp && o.NetAddress.Port == cmd.TcpPort);
            if (otau == null) return $@"OTAU with address {cmd.OtauIp} not found";

            var bopNetworkEventAdded = Mapper.Map<BopNetworkEventAdded>(cmd);
            var lastEventOrdinal = _writeModel.BopNetworkEvents.Any() ? _writeModel.BopNetworkEvents.Max(n => n.Ordinal) : 1;
            bopNetworkEventAdded.Ordinal = lastEventOrdinal + 1;

            return _eventsQueue.Add(bopNetworkEventAdded);
        }
    }
}
