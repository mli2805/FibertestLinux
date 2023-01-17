using Fibertest.OtdrDataFormat;
using Fibertest.StringResources;

namespace WpfCommonViews
{
    public static class ForTableConvertor
    {
        public static string ForStateInTable(this RftsEventTypes rftsEventType, bool isFailed)
        {
            if ((rftsEventType & RftsEventTypes.IsFiberBreak) != 0)
                return Resources.SID_fiber_break;
            if ((rftsEventType & RftsEventTypes.IsNew) != 0)
                return Resources.SID_new;
            if ((rftsEventType & RftsEventTypes.IsFailed) != 0)
                return Resources.SID_fail;
            if ((rftsEventType & RftsEventTypes.IsMonitored) != 0)
                return isFailed ? Resources.SID_fail : Resources.SID_pass;
            if (rftsEventType == RftsEventTypes.None)
                return "";
            //   return Resources.SID_pass;
            return Resources.SID_unexpected_input;
        }
        public static string ForEnabledInTable(this RftsEventTypes rftsEventType)
        {
            if ((rftsEventType & RftsEventTypes.IsNew) != 0)
                return Resources.SID_new;
            if ((rftsEventType & RftsEventTypes.IsMonitored) != 0)
                return Resources.SID_yes;
            if (rftsEventType == RftsEventTypes.None)
                return Resources.SID_pass;
            return Resources.SID_unexpected_input;
        }

        public static string ForTable(this LandmarkCode landmarkCode)
        {
            switch (landmarkCode)
            {
                case LandmarkCode.FiberDistributingFrame: return Resources.SID_Rtu;
                case LandmarkCode.Coupler: return Resources.SID_Closure;
                case LandmarkCode.WiringCloset: return Resources.SID_Cross;
                case LandmarkCode.Manhole: return Resources.SID_Node;
                case LandmarkCode.RemoteTerminal: return Resources.SID_Terminal;
                case LandmarkCode.Other: return Resources.SID_Other;
            }
            return Resources.SID_unexpected_input;
        }

        public static string ForTable(this RftsUniversalParameter uniParam)
        {
            var value = (double)uniParam.Value / uniParam.Scale;
            return $"{value} " + Resources.SID__abs__;
        }

        public static string ForTable(this ShortThreshold threshold)
        {
            if (!threshold.IsAbsolute && threshold.RelativeThreshold == 32767) return "";

            var value = threshold.IsAbsolute ? threshold.AbsoluteThreshold : threshold.RelativeThreshold;
            var str = $@"{value / 1000.0: 0.000} ";
            var result = str + (threshold.IsAbsolute ? Resources.SID__abs__ : Resources.SID__rel__);
            return result;
        }

        public static string EventCodeForTable(this string eventCode)
        {
            var str = eventCode[0] == '0' ? @"S" : @"R";
            return $@"{str} : {eventCode[1]}";
        }


    }
}