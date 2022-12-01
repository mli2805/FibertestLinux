namespace Fibertest.Dto;

public class RequestAnswer
{
    public ReturnCode ReturnCode;
    public string? ErrorMessage;

    public RtuOccupationState? RtuOccupationState;

    public RequestAnswer(ReturnCode returnCode)
    {
        ReturnCode = returnCode;
    }
}