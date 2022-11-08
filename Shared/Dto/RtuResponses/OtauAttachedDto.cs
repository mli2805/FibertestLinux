namespace Fibertest.Dto;

public class OtauAttachedDto : BaseRtuReply
{
    public Guid OtauId;
    public string? Serial;
    public int PortCount;

    public bool IsAttached => ReturnCode == ReturnCode.OtauAttachedSuccessfully;
}