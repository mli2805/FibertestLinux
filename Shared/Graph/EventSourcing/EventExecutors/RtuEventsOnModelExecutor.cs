using AutoMapper;
using Fibertest.Dto;
using GMap.NET;

namespace Fibertest.Graph;

public static class RtuEventsOnModelExecutor
{
    private static readonly IMapper Mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

    public static string? AddRtuAtGpsLocation(this Model model, RtuAtGpsLocationAdded e)
    {
        Node node = new Node()
        {
            NodeId = e.NodeId,
            Position = new PointLatLng(e.Latitude, e.Longitude),
            TypeOfLastAddedEquipment = EquipmentType.Rtu,
            Title = e.Title
        };
        model.Nodes.Add(node);
        Rtu rtu = Mapper.Map<Rtu>(e);
        rtu.ZoneIds.Add(model.Zones.First(z => z.IsDefaultZone).ZoneId);
        model.Rtus.Add(rtu);
        return null;
    }

    public static string? UpdateRtu(this Model model, RtuUpdated e)
    {
        var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
        if (rtu == null)
        {
            return $@"RtuUpdated: RTU {e.RtuId.First6()} not found";
        }
        rtu.Title = e.Title;
        rtu.Comment = e.Comment ?? "";
        var nodeOfRtu = model.Nodes.First(n => n.NodeId == rtu.NodeId);
        nodeOfRtu.Title = e.Title;
        nodeOfRtu.Position = e.Position;
        return null;
    }

    public static string? RemoveRtu(this Model model, RtuRemoved e)
    {
        var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
        if (rtu == null)
        {
            return $@"RtuRemoved: RTU {e.RtuId.First6()} not found";
        }
        var nodeId = rtu.NodeId;

        foreach (var otau in model.Otaus.Where(o => o.RtuId == rtu.Id).ToList())
        {
            model.Otaus.Remove(otau);
        }
        foreach (var trace in model.Traces.Where(t => t.RtuId == rtu.Id).ToList())
        {
            model.CleanTrace(new TraceCleaned() { TraceId = trace.TraceId });
            model.Traces.Remove(trace);
        }

        model.Rtus.Remove(rtu);
        model.RemoveNodeWithAllHisFibersUptoRealNode(nodeId);
        return null;
    }

    public static string? AttachOtau(this Model model, OtauAttached e)
    {
        Otau otau = Mapper.Map<Otau>(e);
        model.Otaus.Add(otau);
        var rtu = model.Rtus.First(r => r.Id == otau.RtuId);

        var otauDto = new OtauDto
        {
            OtauId = otau.Id.ToString(), 
            Serial = otau.Serial, 
            NetAddress = otau.NetAddress, 
            OwnPortCount = otau.PortCount, 
            IsOk = otau.IsOk
        };
        rtu.Children.Add(otau.MasterPort, otauDto);
        rtu.FullPortCount += otau.PortCount;
        rtu.SetOtauState(e.Id, e.IsOk);
        return null;
    }

    public static string? DetachOtau(this Model model, OtauDetached e)
    {
        var otau = model.Otaus.FirstOrDefault(o => o.Id == e.Id);
        if (otau == null)
        {
            return $@"OtauDetached: OTAU {e.Id.First6()} not found";
        }
        var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
        if (rtu == null)
        {
            return $@"OtauDetached: RTU {e.RtuId.First6()} not found";
        }

        foreach (var trace in model.Traces.Where(t => t.OtauPort != null &&
                                                      t.OtauPort.Serial == otau.Serial))
        {
            model.DetachTrace(trace);
        }

        model.VeexTests.RemoveAll(t => t.OtauId == e.Id.ToString());

        rtu.FullPortCount -= otau.PortCount;
        foreach (var port in rtu.Children.Keys.ToList())
        {
            if (rtu.Children[port].NetAddress == otau.NetAddress)
                rtu.Children.Remove(port);
        }

        foreach (var bopNetworkEvent in model.BopNetworkEvents.Where(b =>
                     b.RtuId == e.RtuId && b.OtauIp == otau.NetAddress.Ip4Address && b.TcpPort == otau.NetAddress.Port).ToList())
        {
            model.BopNetworkEvents.Remove(bopNetworkEvent);
        }

        rtu.RemoveOtauState(e.Id);
        model.Otaus.Remove(otau);

        foreach (var measurement in model.ActiveMeasurements.Where(m=> e.TracesOnOtau.Contains(m.TraceId)).ToList())
        {
            model.ActiveMeasurements.Remove(measurement);
        }
        return null;
    }

    public static string? DetachAllTraces(this Model model, AllTracesDetached e)
    {
        foreach (var trace in model.Traces.Where(t => t.RtuId == e.RtuId))
        {
            if (trace.IsAttached)
                model.DetachTrace(trace);
        }

        return null;
    }
}