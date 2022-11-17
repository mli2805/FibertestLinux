using Fibertest.Dto;

namespace Graph
{
    public class ChangeMonitoringSettings
    {
        public Guid RtuId;

        public List<Guid>? TracesInMonitoringCycle;

        public Frequency PreciseMeas;
        public Frequency PreciseSave;
        public Frequency FastSave;

        public bool IsMonitoringOn;
    }
}