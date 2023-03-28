using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class EventStatusComboItem
    {
        public EventStatus EventStatus { get; set; }

        public override string ToString()
        {
            return EventStatus.GetLocalizedString();
        }
    }
}