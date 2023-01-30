using Fibertest.Dto;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class EventStatusFilter
    {
        public bool IsOn { get; set; }
        public EventStatus EventStatus { get; set; }

        public EventStatusFilter() { IsOn = false; }

        public EventStatusFilter(EventStatus eventStatus)
        {
            IsOn = true;
            EventStatus = eventStatus;
        }

        public override string ToString()
        {
            return IsOn ? EventStatus.GetLocalizedString() : Resources.SID__no_filter_;
        }
    }
}