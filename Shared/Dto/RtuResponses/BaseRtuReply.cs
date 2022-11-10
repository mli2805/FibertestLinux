namespace Fibertest.Dto
{
    public class BaseRtuReply
    {
        public ReturnCode ReturnCode;
        public string? ErrorMessage;

        public RtuOccupationState? RtuOccupationState;
    }
}
