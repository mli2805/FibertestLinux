namespace Fibertest.Dto
{
    public enum EventStatus
    {
        JustMeasurementNotAnEvent = -99, // only for trace statistics
        EventButNotAnAccident = -9,  // Ok or Suspicion (made by Fast)

        NotImportant = -3,
        Planned = -2,
        NotConfirmed = -1,
        Unprocessed = 0,
        Suspended = 1,
        Confirmed = 2,
    }
}