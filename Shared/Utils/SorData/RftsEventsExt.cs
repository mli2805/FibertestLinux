using Fibertest.Dto;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Fibertest.Utils
{
    // for WebClient
    public static class RftsEventsExt
    {
        public static string ForStateInTable(this RftsEventTypes rftsEventType, bool isFailed)
        {
            if ((rftsEventType & RftsEventTypes.IsFiberBreak) != 0)
                return "SID_fiber_break";
            if ((rftsEventType & RftsEventTypes.IsNew) != 0)
                return "SID_new";
            if ((rftsEventType & RftsEventTypes.IsFailed) != 0)
                return "SID_fail";
            if ((rftsEventType & RftsEventTypes.IsMonitored) != 0)
                return isFailed ? "SID_fail" : "SID_pass";
            if (rftsEventType == RftsEventTypes.None)
                return "";
            return "SID_unexpected_input";
        }

        public static string ForEnabledInTable(this RftsEventTypes rftsEventType)
        {
            if ((rftsEventType & RftsEventTypes.IsNew) != 0)
                return "SID_new";
            if ((rftsEventType & RftsEventTypes.IsMonitored) != 0)
                return "SID_yes";
            if (rftsEventType == RftsEventTypes.None)
                return "SID_pass";
            return "SID_unexpected_input";
        }

        public static string ForTable(this LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return "SID_Rtu";
                case LandmarkCode.Coupler: return "SID_Closure";
                case LandmarkCode.WiringCloset: return "SID_Cross";
                case LandmarkCode.Manhole: return "SID_Node";
                case LandmarkCode.RemoteTerminal: return "SID_Terminal";
                case LandmarkCode.Other: return "SID_Other";
            }
            return "SID_unexpected_input";
        }

        public static string EventCodeForTable(this string eventCode)
        {
            var str = eventCode[0] == '0' ? @"S" : @"R";
            return $@"{str} : {eventCode[1]}";
        }

        public static string ForDeviationInTable(this RftsEventDto rftsEventDto, ShortDeviation deviation, string letter)
        {
            var formattedValue = $@"{(short)deviation.Deviation / 1000.0: 0.000}";
            if ((deviation.Type & ShortDeviationTypes.IsExceeded) != 0)
            {
                formattedValue += $@" ( {letter} ) ";
                rftsEventDto.IsFailed = true;
                rftsEventDto.DamageType += $@" {letter}";
            }
            return formattedValue;
        }


        public static string ToSid(this RftsLevelType level)
        {
            switch (level)
            {
                case RftsLevelType.Minor: return @"SID_Minor";
                case RftsLevelType.Major: return @"SID_Major";
                case RftsLevelType.Critical: return @"SID_Critical";
                default: return @"SID_User_s";
            }
        }
    }
}