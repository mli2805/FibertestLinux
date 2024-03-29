﻿using GMap.NET;

namespace Fibertest.Graph;

[Serializable]
public class AccidentNeighbour
{
    public int LandmarkIndex;
    public string? Title;
    public PointLatLng Coors;
    public double ToRtuOpticalDistanceKm;
    public double ToRtuPhysicalDistanceKm;
}