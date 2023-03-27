namespace Fibertest.Dto;

public enum MeasurementResult
{
    Success = 0,
    ToggleToPortFailed,
    FastBaseRefNotFound,
    PreciseBaseRefNotFound,
    AdditionalBaseRefNotFound,
    HardwareProblem,
    FailedGetSorBuffer,
    AnalysisFailed,
    ComparisonFailed,
}