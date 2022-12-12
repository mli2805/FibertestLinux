using AutoMapper;
using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class FiberEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        
        public static string? AddFiber(this Model model, FiberAdded e)
        {
            model.Fibers.Add(Mapper.Map<Fiber>(e));
            return null;
        }

        public static string? UpdateFiber(this Model model, FiberUpdated source)
        {
            var destination = model.Fibers.FirstOrDefault(f => f.FiberId == source.Id);
            if (destination == null)
            {
                return $@"FiberUpdated: Fiber {source.Id.First6()} not found";
            }
            Mapper.Map(source, destination);
            return null;
        }

        public static string? RemoveFiber(this Model model, FiberRemoved e)
        {
            var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (fiber == null)
            {
                return $@"FiberRemoved: Fiber {e.FiberId.First6()} not found";
            }
            model.RemoveFiberUptoRealNodesNotPoints(fiber);
            return null;
        }
    }
}