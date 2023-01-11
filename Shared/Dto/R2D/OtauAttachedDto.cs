namespace Fibertest.Dto;

public class OtauAttachedDto : RequestAnswer
{
    public Guid OtauId;
    public Guid RtuId;
    public string? Serial;
    public int PortCount;

    public bool IsAttached => ReturnCode == ReturnCode.OtauAttachedSuccessfully;

    public OtauAttachedDto(){}
    public OtauAttachedDto(ReturnCode returnCode) : base(returnCode)
    {
    }
}