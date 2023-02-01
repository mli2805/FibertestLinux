namespace Fibertest.Dto;

public class GetModelPortionDto : BaseRequest
{
    public int Portion;

    public GetModelPortionDto(int portion)
    {
        Portion = portion;
    }

    public override string What => "GetModelPortion";
}