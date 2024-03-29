﻿using Fibertest.Dto;
using GMap.NET;

namespace Fibertest.Graph;

[Serializable]
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
    public string AccidentTitle = string.Empty;
    public double AccidentToRtuPhysicalDistanceKm;

    public double AccidentToLeftOpticalDistanceKm;
    public double AccidentToLeftPhysicalDistanceKm;
    public double AccidentToRightOpticalDistanceKm;
    public double AccidentToRightPhysicalDistanceKm;

    public string EventCode = null!;
    public double DeltaLen;

    // could be null for accidents in first or last event
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