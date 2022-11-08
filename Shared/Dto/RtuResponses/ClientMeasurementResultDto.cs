namespace Fibertest.Dto;

public class ClientMeasurementResultDto : BaseRtuReply
{
    public Guid ClientMeasurementId;
    public OtauPortDto? OtauPortDto;
    public byte[]? SorBytes;
}