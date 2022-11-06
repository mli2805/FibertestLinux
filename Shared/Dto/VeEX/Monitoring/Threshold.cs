

// ReSharper disable InconsistentNaming

namespace Fibertest.Dto
{
    public class ThresholdSet
    {
        public List<Level>? levels;
    }

    public class Level
    {
        public AdvancedThresholds? advancedThresholds;
        public List<Group>? groups;
        public string? name;
    }

    public class AdvancedThresholds
    {
        public double attenuationCoefficientChangeForNewEvents;
        public double eofAttenuationCoefficientChangeForFiberBreak;
        public double eofLossChangeForFiberBreak;
        public double maxEofAttenuationCoefficientForFiberBreak;
        public double noiseLevelChangeForFiberElongation;
    }

    public class Group
    {
        public Thresholds? thresholds;
    }

    public class Thresholds
    {
        public CombinedThreshold? eventLeadingLossCoefficient;
        public CombinedThreshold? eventLoss;
        public CombinedThreshold? eventMaxLevel;  // PON
        public CombinedThreshold? eventReflectance;
        public CombinedThreshold? nonReflectiveEventPosition; // UI in Advanced
        public CombinedThreshold? reflectiveEventPosition; // UI in Advanced
    }

    public class CombinedThreshold
    {
        public double? min;
        public double? max;
        public double? decrease;
        public double? increase;
    }
}
