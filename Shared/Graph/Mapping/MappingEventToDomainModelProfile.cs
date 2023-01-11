using AutoMapper;

namespace Fibertest.Graph;

public class MappingEventToDomainModelProfile : Profile
{
    public MappingEventToDomainModelProfile()
    {
        CreateMap<NodeUpdated, Node>();
        CreateMap<NodeUpdatedAndMoved, Node>();
        CreateMap<NodeMoved, Node>();

        CreateMap<FiberAdded, Fiber>();
        CreateMap<FiberUpdated, Fiber>();
        CreateMap<FiberRemoved, Fiber>();

        CreateMap<EquipmentIntoNodeAdded, Equipment>();
        CreateMap<EquipmentAtGpsLocationAdded, Equipment>();
        CreateMap<EquipmentAtGpsLocationWithNodeTitleAdded, Equipment>();
        CreateMap<EquipmentUpdated, Equipment>();

        CreateMap<RtuAtGpsLocationAdded, Node>();
        CreateMap<RtuAtGpsLocationAdded, Rtu>();
        CreateMap<RtuInitialized, Rtu>();

        CreateMap<OtauAttached, Otau>();

        CreateMap<TraceAdded, Trace>();
        CreateMap<TraceUpdated, Trace>();

        CreateMap<TceWithRelationsAddedOrUpdated, TceS>();

        CreateMap<ZoneAdded, Zone>();
        CreateMap<ZoneUpdated, Zone>();

        CreateMap<UserAdded, User>();
        CreateMap<UserUpdated, User>();

        CreateMap<MeasurementAdded, Measurement>();
        CreateMap<MeasurementUpdated, Measurement>();
        CreateMap<NetworkEventAdded, NetworkEvent>();
        CreateMap<BopNetworkEventAdded, BopNetworkEvent>();
        CreateMap<LicenseApplied, License>();

        CreateMap<VeexTestAdded, VeexTest>();
    }


}