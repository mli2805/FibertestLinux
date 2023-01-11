namespace Fibertest.Dto;

public class GetLineParametersDto : BaseRtuRequest
{
    public GetLineParametersDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }

    public List<OtauPortDto> OtauPortDto = new List<OtauPortDto>();

    public override string What => "GetLineParameters";
    public override RtuOccupation Why => RtuOccupation.DoMeasurementClient;

}