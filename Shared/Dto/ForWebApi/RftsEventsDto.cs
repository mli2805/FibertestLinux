namespace Fibertest.Dto
{
    public class RftsEventsDto
    {
        public ReturnCode ReturnCode;
        public string? ErrorMessage;

        public bool IsNoFiber;
        public RftsLevelDto[]? LevelArray;
        public RftsEventsSummaryDto? Summary;
    }

    public class RftsLevelDto
    {
        public string? Title;
        public bool IsFailed;
        public string? FirstProblemLocation;
        public RftsEventDto[]? EventArray;
        public TotalFiberLossDto? TotalFiberLoss;
    }

    public class RftsEventDto
    {
        public int Ordinal;
        public bool IsNew;
        public bool IsFailed;

        public string? LandmarkTitle;
        public string? LandmarkType;
        public string? State;
        public string? DamageType;
        public string? DistanceKm;
        public string? Enabled;
        public string? EventType;

        public string? ReflectanceCoeff;
        public string? AttenuationInClosure;
        public string? AttenuationCoeff;

        public MonitoringThreshold? ReflectanceCoeffThreshold;
        public MonitoringThreshold? AttenuationInClosureThreshold;
        public MonitoringThreshold? AttenuationCoeffThreshold;

        public string? ReflectanceCoeffDeviation;
        public string? AttenuationInClosureDeviation;
        public string? AttenuationCoeffDeviation;
    }

    public class TotalFiberLossDto
    {
        public double Value;
        public MonitoringThreshold? Threshold;
        public double Deviation;
        public bool IsPassed;
    }

    public class MonitoringThreshold
    {
        public double Value;
        public bool IsAbsolute;
    }

    public class RftsEventsSummaryDto
    {
        public string? TraceState;
        public double Orl;
        public LevelState[]? LevelStates;
    }

    public class LevelState
    {
        public string? LevelTitle;
        public string? State;
    }
}
