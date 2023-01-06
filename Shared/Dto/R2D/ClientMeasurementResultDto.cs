namespace Fibertest.Dto;

public class ClientMeasurementResultDto : RequestAnswer
{
    public Guid ClientMeasurementId;
    public OtauPortDto? OtauPortDto;
    public byte[]? SorBytes;

    public ClientMeasurementResultDto(ReturnCode returnCode) : base(returnCode)
    {
    }
}