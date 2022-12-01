namespace Fibertest.Dto
{
    public class RtuInitializedWebDto
    {
        public Guid RtuId;

        public ReturnCode ReturnCode;
        public string? ErrorMessage;

        public RtuNetworkSettingsDto? RtuNetworkSettings;
    }
}