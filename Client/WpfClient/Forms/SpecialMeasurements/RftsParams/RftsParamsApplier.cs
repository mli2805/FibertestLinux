using System.Linq;
using Fibertest.OtdrDataFormat;

namespace Fibertest.WpfClient
{
    public static class RftsParamsApplier
    {
        public static void ApplyRftsParamsTemplate(this OtdrDataKnownBlocks sorData, RftsParams rftsParams)
        {
            sorData.RftsParameters.BlockId = @"RFTSParams";

            sorData.RftsParameters.Levels =
                rftsParams.Levels.Select(rftsParamsLevel => rftsParamsLevel.RftsParamsLevelToSorFormat()).ToArray();
            sorData.RftsParameters.LevelsCount = (ushort)rftsParams.LevelNumber;

            sorData.RftsParameters.UniversalParameters =
                rftsParams.UniParams.Select(uniParam => uniParam.ToSorFormat()).ToArray();
            sorData.RftsParameters.UniversalParametersCount = (ushort)rftsParams.UniversalParamNumber;
            sorData.RftsParameters.UseUniversalParameters = true;
        }

        private static RftsLevel RftsParamsLevelToSorFormat(this RftsParamsLevel rftsParamsLevel)
        {
            var levelThresholdSet = rftsParamsLevel.LevelThresholdSet.ToSorFormat();
            var rftsLevel = new RftsLevel()
            {
                LevelName = rftsParamsLevel.LevelName.ToRftsLevelType(),
                IsEnabled = rftsParamsLevel.Enabled,
                LevelThresholdSet = levelThresholdSet,
                EELT = rftsParamsLevel.Eelt.ToSorFormat(),

                // just to have the same result as after Reflect.exe
                ThresholdSets = new []{ levelThresholdSet, levelThresholdSet },
                NumberOfThresholdSets = 2,
            };

            return rftsLevel;
        }

        private static RftsThresholdSet ToSorFormat(this RftsLevelThresholdSet thresholdSet)
        {
            return new RftsThresholdSet()
            {
                AttenuationCoefThreshold = thresholdSet.Ct.ToSorFormat(),
                AttenuationThreshold = thresholdSet.Lt.ToSorFormat(),
                ReflectanceThreshold = thresholdSet.Rt.ToSorFormat(),
            };
        }

        private static ShortThreshold ToSorFormat(this Threshold threshold)
        {
            return new ShortThreshold()
            {
                IsAbsolute = threshold.Absolute,
                AbsoluteThreshold = threshold.AbsoluteThreshold,
                RelativeThreshold = threshold.RelativeThreshold,
            };
        }

        private static RftsLevelType ToRftsLevelType(this string levelName)
        {
            if (levelName == @"minor") return RftsLevelType.Minor;
            if (levelName == @"major") return RftsLevelType.Major;
            if (levelName == @"critical") return RftsLevelType.Critical;
            return RftsLevelType.None;
        }

        private static RftsUniversalParameter ToSorFormat(this RftsUniParameter uniParameter)
        {
            return new RftsUniversalParameter()
            {
                Name = uniParameter.Name,
                Value = uniParameter.Value,
                Scale = uniParameter.Scale,
            };
        }
    }
}
