namespace Fibertest.Dto;

public class AccidentInSor
{
    public int BrokenRftsEventNumber;

    public FiberState AccidentSeriousness;
    public OpticalAccidentType OpticalTypeOfAccident;

    public bool IsAccidentInOldEvent;
    public double AccidentToRtuOpticalDistanceKm;

    public string? EventCode;
    public double DeltaLen;

    public override string ToString()
    {
        string str = IsAccidentInOldEvent ?  $"old event {BrokenRftsEventNumber}" : $"new event {BrokenRftsEventNumber}";
        return $"{AccidentSeriousness} ({OpticalTypeOfAccident.ToLetter()}) in {str} code " +
               $"{EventCode} at {AccidentToRtuOpticalDistanceKm:0.0000} delta {DeltaLen:0.0000}";
    }

    public bool IsTheSame(AccidentInSor other)
    {
        if (BrokenRftsEventNumber != other.BrokenRftsEventNumber) return false;
        if (AccidentSeriousness != other.AccidentSeriousness) return false;
        if (OpticalTypeOfAccident != other.OpticalTypeOfAccident) return false;
        if (IsAccidentInOldEvent != other.IsAccidentInOldEvent) return false;

        return Math.Abs(AccidentToRtuOpticalDistanceKm - other.AccidentToRtuOpticalDistanceKm) < DeltaLen;
    }
}