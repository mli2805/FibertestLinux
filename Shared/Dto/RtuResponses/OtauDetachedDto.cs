namespace Fibertest.Dto;

public class OtauDetachedDto : RequestAnswer
{
    public Guid OtauId;
    public Guid RtuId;

    public bool IsDetached => ReturnCode == ReturnCode.OtauDetachedSuccessfully;

    public OtauDetachedDto(ReturnCode returnCode) : base(returnCode)
    {
    }
}