namespace Fibertest.Dto;

public class MoniResult
{
    public MeasurementResult MeasurementResult;
    public bool IsInterrupted;
    public bool IsNoFiber;
    public bool IsFiberBreak;

    public List<MoniLevel> Levels = new();

    public BaseRefType BaseRefType;
    public double FirstBreakDistance;

    public List<AccidentInSor> Accidents = new();
    public byte[]? SorBytes;

    public FiberState GetAggregatedResult()
    {
        if (IsInterrupted)
            return FiberState.Unknown;

        if (IsNoFiber)
            return FiberState.NoFiber;
        if (IsFiberBreak)
            return FiberState.FiberBreak;

        var lvl = Levels.LastOrDefault(l => l.IsLevelFailed);
        return lvl == null ? FiberState.Ok : (FiberState) (int) lvl.Type;
    }

    public bool IsStateChanged(MoniResult? previous)
    {
        if (previous == null) return true;
        var currentState = GetAggregatedResult();
        if (previous.GetAggregatedResult() != currentState)
            return true;

        if (currentState == FiberState.NoFiber || currentState == FiberState.Ok)
            return false;

        if (previous.Accidents.Count != Accidents.Count)
            return true;

        for (int i = 0; i < Accidents.Count; i++)
        {
            if (!Accidents[i].IsTheSame(previous.Accidents[i])) return true;
        }

        return false;
    }
}