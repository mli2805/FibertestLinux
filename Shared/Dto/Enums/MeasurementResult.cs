namespace Fibertest.Dto;

public enum MeasurementResult
{
    Success = 0,
    ToggleToPortFailed,
    FastBaseRefNotFound,
    PreciseBaseRefNotFound,
    AdditionalBaseRefNotFound,
    HardwareProblem,
    Interrupted,
    FailedGetSorBuffer,
    AnalysisFailed,
    ComparisonFailed,
}