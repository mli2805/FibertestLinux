namespace Fibertest.Dto;

public class GetSorBytesDto : BaseRequest
{
    public int SorFileId;

    public override string What => "GetSorBytes";

}