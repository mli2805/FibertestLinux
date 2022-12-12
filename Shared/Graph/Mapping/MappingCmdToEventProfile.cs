using AutoMapper;

namespace Fibertest.Graph
{
    public class MappingCmdToEventProfile : Profile
    {
        public MappingCmdToEventProfile()
        {
            CreateMap<AddNodeIntoFiber, NodeIntoFiberAdded>();
            CreateMap<UpdateNode, NodeUpdated>();
            CreateMap<UpdateAndMoveNode, NodeUpdatedAndMoved>();
            CreateMap<MoveNode, NodeMoved>();
            CreateMap<RemoveNode, NodeRemoved>();

            CreateMap<AddEquipmentIntoNode, EquipmentIntoNodeAdded>();
            CreateMap<AddEquipmentAtGpsLocation, EquipmentAtGpsLocationAdded>();
            CreateMap<AddEquipmentAtGpsLocationWithNodeTitle, EquipmentAtGpsLocationWithNodeTitleAdded>();
            CreateMap<UpdateEquipment, EquipmentUpdated>();
            CreateMap<RemoveEquipment, EquipmentRemoved>();
            CreateMap<IncludeEquipmentIntoTrace, EquipmentIntoTraceIncluded>();
            CreateMap<ExcludeEquipmentFromTrace, EquipmentFromTraceExcluded>();

            CreateMap<AddFiber, FiberAdded>();
            CreateMap<UpdateFiber, FiberUpdated>();
            CreateMap<RemoveFiber, FiberRemoved>();

            CreateMap<RemoveUnused, UnusedRemoved>();

            CreateMap<AddRtuAtGpsLocation, RtuAtGpsLocationAdded>();
            CreateMap<UpdateRtu, RtuUpdated>();
            CreateMap<RemoveRtu, RtuRemoved>();
            CreateMap<AttachOtau, OtauAttached>();
            CreateMap<DetachOtau, OtauDetached>();
            CreateMap<DetachAllTraces, AllTracesDetached>();

            CreateMap<AddOrUpdateTceWithRelations, TceWithRelationsAddedOrUpdated>();
            CreateMap<RemoveTce, TceRemoved>();
            CreateMap<ReSeedTceTypeStructList, TceTypeStructListReSeeded> ();

            CreateMap<InitializeRtu, RtuInitialized>();
            CreateMap<ChangeMonitoringSettings, MonitoringSettingsChanged>();
            CreateMap<AssignBaseRef, BaseRefAssigned>();
            CreateMap<StartMonitoring, MonitoringStarted>();
            CreateMap<StopMonitoring, MonitoringStopped>();

            CreateMap<AttachTrace, TraceAttached>();
            CreateMap<DetachTrace, TraceDetached>();

            CreateMap<AddTrace, TraceAdded>();
            CreateMap<UpdateTrace, TraceUpdated>();
            CreateMap<CleanTrace, TraceCleaned>();
            CreateMap<RemoveTrace, TraceRemoved>();
            CreateMap<AssignBaseRef, BaseRefAssigned>();

            CreateMap<AddZone, ZoneAdded>();
            CreateMap<UpdateZone, ZoneUpdated>();
            CreateMap<RemoveZone, ZoneRemoved>();

            CreateMap<AddUser, UserAdded>();
            CreateMap<UpdateUser, UserUpdated>();
            CreateMap<AssignUsersMachineKey, UsersMachineKeyAssigned>();
            CreateMap<RemoveUser, UserRemoved>();

            CreateMap<AddMeasurement, MeasurementAdded>();
            CreateMap<UpdateMeasurement, MeasurementUpdated>();
            CreateMap<AddNetworkEvent, NetworkEventAdded>();
            CreateMap<AddBopNetworkEvent, BopNetworkEventAdded>();
            CreateMap<ChangeResponsibilities, ResponsibilitiesChanged>();
            CreateMap<ApplyLicense, LicenseApplied>();
            CreateMap<MakeSnapshot, SnapshotMade>();

            CreateMap<RemoveEventsAndSors, EventsAndSorsRemoved>();

            CreateMap<RegisterClientStation, ClientStationRegistered>();

            CreateMap<AddVeexTest, VeexTestAdded>();
            CreateMap<RemoveVeexTest, VeexTestRemoved>();
        }
    }
}