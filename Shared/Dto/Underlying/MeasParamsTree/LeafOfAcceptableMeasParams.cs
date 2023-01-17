namespace Fibertest.Dto;

[Serializable]
public class LeafOfAcceptableMeasParams
{
    public string[] Resolutions = null!;
    public string[] PulseDurations = null!;
    public string[] PeriodsToAverage = null!;
    public string[] MeasCountsToAverage = null!;
}