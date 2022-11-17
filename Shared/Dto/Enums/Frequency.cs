namespace Fibertest.Dto
{
    public enum Frequency
    {
        DoNot        = 9999,
        EveryHour    = 1,
        Every6Hours  = 6,
        Every12Hours = 12,
        EveryDay     = 24,
        Every2Days   = 48,
        Every7Days   = 168,
        Every30Days  = 720,
    }

    public static class FrequencyExt
    {
        public static TimeSpan GetTimeSpan(this Frequency frequency)
        {
            return frequency == Frequency.DoNot ? TimeSpan.Zero : TimeSpan.FromHours((int) frequency);
        }

        public static Frequency GetFrequency(this TimeSpan timeSpan)
        {
            return timeSpan == TimeSpan.Zero ? Frequency.DoNot : (Frequency) timeSpan.TotalHours;
        }
    }
}