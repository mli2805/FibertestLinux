using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public static class RftsParamsSaver
    {
        public static void Save(this RftsParams rftsParams, string filename)
        {
            var lines = new List<string>() { @"TNL", rftsParams.LevelNumber.ToString() };
            foreach (var level in rftsParams.Levels)
                lines.AddRange(level.ForFile());
            lines.AddRange(new List<string>() { @"TNUP", rftsParams.UniversalParamNumber.ToString() });
            foreach (var uniParam in rftsParams.UniParams)
                lines.AddRange(uniParam.ForFile());

            File.WriteAllLines(filename, lines);
        }

        private static List<string> ForFile(this RftsParamsLevel level)
        {
            var result = new List<string>() { @"LevelName", level.LevelName, @"Enabled", level.Enabled ? 1.ToString() : 0.ToString() };
            result.AddRange(level.LevelThresholdSet.ForFile());
            result.AddRange(level.Eelt.ForFile().ToList());
            return result;
        }

        private static List<string> ForFile(this RftsLevelThresholdSet thresholdSet)
        {
            var result = thresholdSet.Lt.ForFile().ToList();
            result.AddRange(thresholdSet.Rt.ForFile());
            result.AddRange(thresholdSet.Ct.ForFile());
            return result;
        }

        private static IEnumerable<string> ForFile(this Threshold threshold)
        {
            yield return @"Absolute";
            yield return threshold.Absolute ? 1.ToString() : 0.ToString();
            yield return @"AbsoluteThreshold";
            yield return threshold.AbsoluteThreshold.ToString();
            yield return @"RelativeThreshold";
            yield return threshold.RelativeThreshold.ToString();
        }

        private static IEnumerable<string> ForFile(this RftsUniParameter uniParameter)
        {
            yield return @"Name";
            yield return uniParameter.Name + @" " + uniParameter.Name.GetUniParamLocalizedName();
            yield return @"Value";
            yield return uniParameter.Value.ToString();
            yield return @"Scale";
            yield return uniParameter.Scale.ToString();
        }

        private static string GetUniParamLocalizedName(this string name)
        {
            switch (name)
            {
                case "EvtDetectDeltaCT": return Resources.SID_EvtDetectDeltaCT;
                case "EvtSearchStep": return Resources.SID_EvtSearchStep;
                case "EvtDetectDeltaLen": return Resources.SID_EvtDetectDeltaLen;
                case "EvtRDetectDeltaLen": return Resources.SID_EvtRDetectDeltaLen;
                case "AutoLT": return Resources.SID_AutoLT;
                case "AutoRT": return Resources.SID_AutoRT;
                case "AutoET": return Resources.SID_AutoET;
                case "NoLinkDistance": return Resources.SID_NoLinkDistance;
                case "NoLinkDeltaDB": return Resources.SID_NoLinkDeltaDB;
                case "EventRT": return Resources.SID_EventRT;
                case "EOFLMN": return Resources.SID_EOFLMN;
                case "CorrectMarkers": return Resources.SID_CorrectMarkers;
                case "EvtChangeLT": return Resources.SID_EvtChangeLT;
                case "EvtChangeRT": return Resources.SID_EvtChangeRT;
                case "EvtChangeCT": return Resources.SID_EvtChangeCT;
                case "EvtChangeET": return Resources.SID_EvtChangeET;
                default: return @"unknown";
            }
        }
    }
}
