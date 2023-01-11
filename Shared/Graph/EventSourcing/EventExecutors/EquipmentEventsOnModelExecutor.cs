using AutoMapper;
using Fibertest.Dto;

namespace Fibertest.Graph;

public static class EquipmentEventsOnModelExecutor
{
    private static readonly IMapper Mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        
    public static string? AddEquipmentIntoNode(this Model model, EquipmentIntoNodeAdded e)
    {
        var node = model.Nodes.First(n => n.NodeId == e.NodeId);
        node.TypeOfLastAddedEquipment = e.Type;
        Equipment equipment = Mapper.Map<Equipment>(e);
        model.Equipments.Add(equipment);
        foreach (var traceId in e.TracesForInsertion)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
            {
                return $@"EquipmentIntoNodeAdded: Trace {traceId.First6()} not found";
            }
            var idx = trace.NodeIds.IndexOf(e.NodeId);
            trace.EquipmentIds[idx] = e.EquipmentId;
        }
        return null;
    }

    public static string? AddEquipmentAtGpsLocation(this Model model, EquipmentAtGpsLocationAdded e)
    {
        Node node = new Node()
        {
            NodeId = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude), TypeOfLastAddedEquipment = e.Type
        };
        model.Nodes.Add(node);
        Equipment equipment = Mapper.Map<Equipment>(e);
        equipment.EquipmentId = e.RequestedEquipmentId;
        model.Equipments.Add(equipment);
        if (e.EmptyNodeEquipmentId != Guid.Empty)
        {
            Equipment emptyEquipment = Mapper.Map<Equipment>(e);
            emptyEquipment.EquipmentId = e.EmptyNodeEquipmentId;
            emptyEquipment.Type = EquipmentType.EmptyNode;
            model.Equipments.Add(emptyEquipment);
        }
        return null;
    }

    public static string? AddEquipmentAtGpsLocationWithNodeTitle(this Model model, EquipmentAtGpsLocationWithNodeTitleAdded e)
    {
        if (model.Nodes.Any(i => i.NodeId == e.NodeId)) // kadastr loader
            return @"node with the same id found";

        model.Nodes.Add(new Node() { NodeId = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude),
            TypeOfLastAddedEquipment = e.Type, Title = e.Title, Comment = e.Comment });

        if (e.RequestedEquipmentId != Guid.Empty)
            model.Equipments.Add(new Equipment() { EquipmentId = e.RequestedEquipmentId, Type = e.Type, NodeId = e.NodeId });

        if (e.EmptyNodeEquipmentId != Guid.Empty)
            model.Equipments.Add(new Equipment() { EquipmentId = e.EmptyNodeEquipmentId, Type = EquipmentType.EmptyNode, NodeId = e.NodeId });

        return null;
    }

    public static string? UpdateEquipment(this Model model, EquipmentUpdated e)
    {
        var equipment = model.Equipments.FirstOrDefault(eq => eq.EquipmentId == e.EquipmentId);
        if (equipment == null)
        {
            return $@"EquipmentUpdated: Equipment {e.EquipmentId.First6()} not found";
        }
        var node = model.Nodes.FirstOrDefault(n => n.NodeId == equipment.NodeId);
        if (node == null)
        {
            return $@"EquipmentUpdated: Node for equipment {e.EquipmentId.First6()} not found";
        }
        node.TypeOfLastAddedEquipment = e.Type;
        Mapper.Map(e, equipment);
        return null;
    }

    public static string? RemoveEquipment(this Model model, EquipmentRemoved e)
    {
        var equipment = model.Equipments.FirstOrDefault(eq => eq.EquipmentId == e.EquipmentId);
        if (equipment == null)
        {
            return $@"EquipmentRemoved: Equipment {e.EquipmentId.First6()} not found";
        }

        var emptyEquipment = model.Equipments.FirstOrDefault(eq => eq.NodeId == equipment.NodeId && eq.Type == EquipmentType.EmptyNode);
        if (emptyEquipment == null)
        {
            return $@"EquipmentRemoved: There is no empty equipment in node {equipment.NodeId.First6()}";
        }

        var traces = model.Traces.Where(t => t.EquipmentIds.Contains(e.EquipmentId)).ToList();
        foreach (var trace in traces)
        {
            while (true)
            {
                var idx = trace.EquipmentIds.IndexOf(e.EquipmentId);
                if (idx == -1) break;
                trace.EquipmentIds[idx] = emptyEquipment.EquipmentId;
            }
        }

        var node = model.Nodes.FirstOrDefault(n => n.NodeId == equipment.NodeId);
        if (node == null)
        {
            return $@"RemoveEquipment: Node for equipment {e.EquipmentId.First6()} not found";
        }
        model.Equipments.Remove(equipment);
        node.TypeOfLastAddedEquipment = model.Equipments.Where(p => p.NodeId == node.NodeId).Max(q => q.Type);
        return null;
    }

    public static string? IncludeEquipmentIntoTrace(this Model model, EquipmentIntoTraceIncluded e)
    {
        var trace = model.Traces.First(t => t.TraceId == e.TraceId);
        trace.EquipmentIds[e.IndexInTrace] = e.EquipmentId;
        return null;
    }

    public static string? ExcludeEquipmentFromTrace(this Model model, EquipmentFromTraceExcluded e)
    {
        var trace = model.Traces.First(t => t.TraceId == e.TraceId);
        var nodeId = trace.NodeIds[e.IndexInTrace];
        var emptyEquipment = model.Equipments.FirstOrDefault(eq => eq.NodeId == nodeId && eq.Type == EquipmentType.EmptyNode);
        if (emptyEquipment == null)
        {
            return $@"EquipmentRemoved: There is no empty equipment in node {nodeId.First6()}";
        }

        var emptyEqId = emptyEquipment.EquipmentId;
        trace.EquipmentIds[e.IndexInTrace] = emptyEqId;
        return null;
    }
}