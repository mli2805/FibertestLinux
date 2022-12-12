using AutoMapper;

namespace Fibertest.Graph
{
    public static class ZoneEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        
        public static string? AddZone(this Model model, ZoneAdded e)
        {
            model.Zones.Add(Mapper.Map<Zone>(e));
            return null;
        }

        public static string? UpdateZone(this Model model, ZoneUpdated source)
        {
            var destination =  model.Zones.First(f => f.ZoneId == source.ZoneId);
            Mapper.Map(source, destination);
            return null;
        }

        public static string? RemoveZone(this Model model, ZoneRemoved e)
        {
            foreach (var trace in model.Traces)
            {
                if (trace.ZoneIds.Contains(e.ZoneId))
                    trace.ZoneIds.Remove(e.ZoneId);
            }

            foreach (var rtu in model.Rtus)
            {
                if (rtu.ZoneIds.Contains(e.ZoneId))
                    rtu.ZoneIds.Remove(e.ZoneId);
            }

            foreach (var user in model.Users.Where(u=>u.ZoneId == e.ZoneId).ToList())
                model.Users.Remove(user);

            model.Zones.Remove( model.Zones.First(f => f.ZoneId == e.ZoneId));
            return null;
        }

        
    }
}