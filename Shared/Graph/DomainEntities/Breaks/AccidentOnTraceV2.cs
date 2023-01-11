using Fibertest.Dto;

namespace Fibertest.Graph;

public class AccidentOnTraceV2
{
    public int BrokenRftsEventNumber;

    public FiberState AccidentSeriousness;
    public OpticalAccidentType OpticalTypeOfAccident;
        
    public bool IsAccidentInOldEvent;
    public bool IsAccidentInLastNode;
    public PointLatLng AccidentCoors;

    public int AccidentLandmarkIndex;
    public double AccidentToRtuOpticalDistanceKm;
    public string? AccidentTitle;
    public double AccidentToRtuPhysicalDistanceKm;

    public double AccidentToLeftOpticalDistanceKm;
    public double AccidentToLeftPhysicalDistanceKm;
    public double AccidentToRightOpticalDistanceKm;
    public double AccidentToRightPhysicalDistanceKm;

    public string? EventCode;
    public double DeltaLen;

    public AccidentNeighbour? Left;
    public AccidentNeighbour? Right;

    public bool IsTheSame(AccidentOnTraceV2 other)
    {
        if (BrokenRftsEventNumber != other.BrokenRftsEventNumber) return false;
        if (AccidentSeriousness != other.AccidentSeriousness) return false;
        if (OpticalTypeOfAccident != other.OpticalTypeOfAccident) return false;
        if (IsAccidentInOldEvent != other.IsAccidentInOldEvent) return false;

        return Math.Abs(AccidentToRtuOpticalDistanceKm - other.AccidentToRtuOpticalDistanceKm) < DeltaLen;
    }
}