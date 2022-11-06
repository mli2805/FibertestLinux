// ReSharper disable InconsistentNaming

namespace Fibertest.Dto
{
    public class Linkmap
    {
        public string? self;
    }

    public class Report
    {
        public string? self;
    }

    public class Traces
    {
        public string? self;
    }


    public class CompletedTest
    {
        public string? extendedResult;
        public int id; // completed test ID
        public int[]? indicesOfReferenceTraces;
        public Linkmap? linkmap;
        public string? reason;
        public Failure? failure;
        public Report? report;
        public string? result;
        public DateTime started;
        public Guid testId;
        public TraceChange? traceChange;
        public Traces? traces;
        public string? type;
    }

    public class CompletedTestPortion
    {
        public List<CompletedTest>? items;
        public int total;
    }

    public static class CompletedTestExt
    {
        public static MonitoringCurrentStep GetMonitoringCurrentStep(this CompletedTest completedTest)
        {
            if (completedTest.extendedResult != null)
            {
                if (completedTest.extendedResult.StartsWith("otdr"))
                    return MonitoringCurrentStep.FailedOtdrProblem;
                if (completedTest.extendedResult.StartsWith("otau"))
                    return MonitoringCurrentStep.FailedOtauProblem;
            }

            return MonitoringCurrentStep.MeasurementFinished;
        }
    }
}
