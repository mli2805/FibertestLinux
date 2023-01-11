using AutoMapper;

namespace Fibertest.Graph;

public static class VeexTestsEventsOnModelExecutor
{
    private static readonly IMapper Mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

    public static string? AddVeexTest(this Model model, VeexTestAdded e)
    {
        var oldTestsForThisTrace = model.VeexTests.Where(t => t.TraceId == e.TraceId && t.BasRefType == e.BasRefType).ToList();
        foreach (var veexTest in model.VeexTests)
        {
            if (model.Traces.All(t=>t.TraceId != veexTest.TraceId))
                oldTestsForThisTrace.Add(veexTest);
        }
        model.VeexTests.RemoveAll(t=>oldTestsForThisTrace.Contains(t));
            
        //   if (model.VeexTests.All(t => t.TestId != e.TestId))
        model.VeexTests.Add(Mapper.Map<VeexTest>(e));
           
        return null;
    }

    public static string? RemoveVeexTest(this Model model, VeexTestRemoved e)
    {
        var test = model.VeexTests.FirstOrDefault(t => t.TestId == e.TestId);
        if (test != null)
            model.VeexTests.Remove(test);
        return null;
    }
}