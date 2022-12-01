// ReSharper disable InconsistentNaming

namespace Fibertest.Dto;

public class VeexMeasurementResult
{
    public List<ConnectionQuality>? connectionQualities;
    public string? id;
    public LinkObject? report;
    public string? status;
    public string? extendedStatus;
    public Failure? failure;
    public LinkObject? traces;
}