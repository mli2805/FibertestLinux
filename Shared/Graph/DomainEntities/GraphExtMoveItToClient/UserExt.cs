using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class UserExt
    {
        public static string FlipFlop(string before)
        {
            return string.IsNullOrEmpty(before) 
                ? "" 
                : before.Substring(before.Length - 1, 1) + FlipFlop(before.Substring(0, before.Length - 1));
        }

        public static bool ShouldReceiveThisSms(this User user, MonitoringResultDto dto)
        {
            if (user.Sms == null || !user.Sms.IsActivated) return false;
            if (!user.Sms.ShouldReceiveThisTypeOfEvent(dto.TraceState)) return false;
            return true;
        }

        public static bool ShouldReceiveNetworkEventSms(this User user)
        {
            if (user.Sms == null || !user.Sms.IsActivated) return false;
            if (!user.Sms.IsNetworkEventsOn) return false;
            return true;
        }

        public static bool ShouldReceiveBopEventSms(this User user)
        {
            if (user.Sms == null || !user.Sms.IsActivated) return false;
            if (!user.Sms.IsBopEventsOn) return false;
            return true;
        }

        private static bool ShouldReceiveThisTypeOfEvent(this SmsReceiver smsReceiver, FiberState traceState)
        {
            switch (traceState)
            {
                case FiberState.NoFiber:
                case FiberState.FiberBreak:
                    return smsReceiver.IsFiberBreakOn;
                case FiberState.Critical:
                    return smsReceiver.IsCriticalOn;
                case FiberState.Major:
                    return smsReceiver.IsMajorOn;
                case FiberState.Minor:
                    return smsReceiver.IsMinorOn;
                case FiberState.Ok:
                    return smsReceiver.IsOkOn;
                default: return false;
            }
        }
    }
}