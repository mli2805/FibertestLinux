namespace Fibertest.Dto;

public class ClientMeasurementStartedDto : RequestAnswer
{
    public Guid ClientMeasurementId;
        
    public Guid TraceId;
    public OtauPortDto? OtauPortDto;

    public ClientMeasurementStartedDto() { }

    public ClientMeasurementStartedDto(ReturnCode returnCode) : base(returnCode)
    {
    }
}