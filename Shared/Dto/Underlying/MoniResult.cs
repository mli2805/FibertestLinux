namespace Fibertest.Dto;

public class MoniResult
{
    public MeasurementResult MeasurementResult;

    public bool IsNoFiber;
    public bool IsFiberBreak;
    public List<MoniLevel> Levels = new();

    public BaseRefType BaseRefType;
    public double FirstBreakDistance;

    public List<AccidentInSor> Accidents = new();
    public byte[]? SorBytes;

    public FiberState GetAggregatedResult()
    {
        if (MeasurementResult == MeasurementResult.Interrupted) // ? what about other problems during last measurement
            return FiberState.Unknown;

        if (IsNoFiber)
            return FiberState.NoFiber;
        if (IsFiberBreak)
            return FiberState.FiberBreak;

        var lvl = Levels.LastOrDefault(l => l.IsLevelFailed);
        return lvl == null ? FiberState.Ok : (FiberState) (int) lvl.Type;
    }
}