namespace Fibertest.Dto;

public class ChangeDcConfigDto : BaseRequest
{
    public DataCenterConfig NewConfig = null!;

    public override string What => "RegisterClient";

}