using System.Collections.Generic;
using System.Globalization;

namespace Fibertest.WpfClient
{
    public class RftsParams
    {
        public int LevelNumber;
        public List<RftsParamsLevel> Levels = new List<RftsParamsLevel>();
        public int UniversalParamNumber;
        public List<RftsUniParameter> UniParams = new List<RftsUniParameter>();
    }
    
    public class RftsParamsLevel
    {
        public string? LevelName;
        public bool Enabled; // stored as 0 or 1
        public RftsLevelThresholdSet? LevelThresholdSet;
        public Threshold? Eelt;
    }

    public class RftsLevelThresholdSet
    {
        public Threshold? Lt;
        public Threshold? Rt;
        public Threshold? Ct;
    }

    public class Threshold
    {
        public bool Absolute; // stored as 0 or 1
        public int AbsoluteThreshold;
        public int RelativeThreshold;
    }

    public class RftsUniParameter
    {
        public string? Name;
        public int Value;
        public int Scale;
        public string? Comment;

        public override string ToString()
        {
            return ((double)Value / Scale).ToString(CultureInfo.InvariantCulture);
        }
        public void Set(double value)
        {
            Value = (int)(value * 10000);
            Scale = 10000;
        }
    }
}
