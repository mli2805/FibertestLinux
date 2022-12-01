using Fibertest.Dto;

namespace Fibertest.Graph;

public class Fiber
{
    public Guid FiberId;
    public Guid NodeId1;
    public Guid NodeId2;

    public double UserInputedLength;
    public double OpticalLength;

    // if empty - fiber is not in any trace; pair contains trace.TraceId : trace.TraceState
    public Dictionary<Guid, FiberState> States = new();

    public void SetState(Guid traceId, FiberState traceState)
    {
        if (States.ContainsKey(traceId))
            States[traceId] = traceState;
        else
            States.Add(traceId, traceState);
    }

    public void RemoveState(Guid traceId)
    {
        if (States.ContainsKey(traceId))
            States.Remove(traceId);
    }

    public readonly List<Guid> HighLights = new();

    public void SetLightOnOff(Guid traceId, bool light)
    {
        if (light && !HighLights.Contains(traceId))
        {
            HighLights.Add(traceId);
        }

        if (!light && HighLights.Contains(traceId))
        {
            HighLights.Remove(traceId);
        }
    }

    public Dictionary<Guid, FiberState> TracesWithExceededLossCoeff = new();

    public void SetBadSegment(Guid traceId, FiberState lossCoeffSeriousness)
    {
        if (TracesWithExceededLossCoeff.ContainsKey(traceId))
            TracesWithExceededLossCoeff[traceId] = lossCoeffSeriousness; 
        else
            TracesWithExceededLossCoeff.Add(traceId, lossCoeffSeriousness);
    }

    public void RemoveBadSegment(Guid traceId)
    {
        if (TracesWithExceededLossCoeff.ContainsKey(traceId))
            TracesWithExceededLossCoeff.Remove(traceId);
    }

}