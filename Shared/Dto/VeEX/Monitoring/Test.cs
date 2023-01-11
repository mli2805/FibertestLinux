// ReSharper disable InconsistentNaming

namespace Fibertest.Dto;

public class Test
{
    public string? id;
    public string? name;
    public string? state;
    public string? otdrId;
    public List<VeexOtauPort>? otauPorts;
    public int? period;
    public int? failedPeriod;

    public LinkObject? analysis_parameters;
    public LinkObject? thresholds;
    public RelationItems? relations;
    public LinkObject? reference;
    public LinkObject? lastFailed;
    public LinkObject? lastPassed;
}