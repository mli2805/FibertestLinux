using System.Collections.Concurrent;

namespace Fibertest.DataCenter;

public enum DataCenterState
{
    IsInDbOptimizationMode,
}
        
public class GlobalState
{
    private ConcurrentDictionary<DataCenterState, bool> Dict { get; set; } = new ConcurrentDictionary<DataCenterState, bool>();

    public bool IsDatacenterInDbOptimizationMode
    {
        get => Dict.TryGetValue(DataCenterState.IsInDbOptimizationMode, out bool _);
        set
        {
            if (value)
                Dict.TryAdd(DataCenterState.IsInDbOptimizationMode, true);
            else
                Dict.TryRemove(DataCenterState.IsInDbOptimizationMode, out bool _);
        }
    }
}