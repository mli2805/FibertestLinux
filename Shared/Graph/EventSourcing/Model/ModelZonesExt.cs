namespace Fibertest.Graph;

public static class ModelZonesExt
{
    public static string? ChangeResponsibilities(this Model model, ResponsibilitiesChanged e)
    {
        foreach (var pair in e.ResponsibilitiesDictionary)
        {
            var rtu = model.Rtus.FirstOrDefault(r => r.Id == pair.Key);
            if (rtu != null)
            {
                rtu.ZoneIds = ApplyChanges(rtu.ZoneIds, pair.Value);
                continue;
            }

            var trace = model.Traces.First(t => t.TraceId == pair.Key);
            trace.ZoneIds = ApplyChanges(trace.ZoneIds, pair.Value);
        }

        return null;
    }

    private static List<Guid> ApplyChanges(List<Guid> oldList, List<Guid> changes)
    {
        var newList = oldList;
        foreach (var zoneId in changes)
        {
            if (newList.Contains(zoneId))
                newList.Remove(zoneId);
            else
                newList.Add(zoneId);
        }

        return newList;
    }
}