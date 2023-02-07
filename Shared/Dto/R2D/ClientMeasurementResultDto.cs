namespace Fibertest.Dto;

public class ClientMeasurementResultDto : BaseRequest
{
    public ReturnCode ReturnCode { get; set; }

    public Guid ClientMeasurementId;
    public OtauPortDto? OtauPortDto;
    public byte[]? SorBytes;

    public ClientMeasurementResultDto() {}
    public ClientMeasurementResultDto(ReturnCode returnCode)
    {
        ReturnCode = returnCode;
    }
}