

// ReSharper disable InconsistentNaming
namespace Fibertest.Dto;

public class VeexMeasurementRequest
{
    public string? id;
    public string? otdrId;
    public VeexMeasOtdrParameters? otdrParameters;
    public GeneralParameters generalParameters = new GeneralParameters();
    public AnalysisParameters analysisParameters = new AnalysisParameters();
    public SpanParameters spanParameters = new SpanParameters();
    public bool suspendMonitoring;
    public List<VeexOtauPort>? otauPorts;
}