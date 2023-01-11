// ReSharper disable InconsistentNaming

namespace Fibertest.Dto;

public class AnalysisParameters
{
    public double macrobendThreshold;
    public bool findOnlyFirstAndLastEvents;
    public bool setUpIitEvents;
    public List<LasersParameter> lasersParameters = new List<LasersParameter>();
}