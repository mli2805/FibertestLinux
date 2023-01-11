namespace Fibertest.Dto;

public enum MonitoringCurrentStep
{
    Idle,
    Toggle,
    Measure,
    FailedOtauProblem,
    FailedOtdrProblem,
    Interrupted,
    Analysis,
    MeasurementFinished,
}