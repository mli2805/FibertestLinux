namespace Fibertest.Dto;

public class DoClientMeasurementDto : BaseRtuRequest
{
    public DoClientMeasurementDto(Guid rtuId, RtuMaker rtuMaker) : base(rtuId, rtuMaker)
    {
    }
        
    public string OtdrId = string.Empty; //  in VeEX RTU main OTDR has its own ID
        
    public List<MeasParamByPosition>? SelectedMeasParams;
    public VeexMeasOtdrParameters VeexMeasOtdrParameters = new VeexMeasOtdrParameters();
    public AnalysisParameters AnalysisParameters = new AnalysisParameters();
        
    public List<OtauPortDto> OtauPortDto = new List<OtauPortDto>();

    // true - apply semi-analysis (only start/end and one section between them (for auto base ref mode)
    // false - apply full auto analysis (usual client measurement)
    public bool IsForAutoBase;

    // only for AutoBase: if true - apply InsertIitEvents (установить воротики)
    public bool IsInsertNewEvents;

    // false - AutoBase for one trace only
    // true - when AutoBase for whole RTU, DO NOT disconnect from OTDR between measurements
    public bool KeepOtdrConnection;
        
    public bool IsAutoLmax;

    public override string What => "DoClientMeasurement";
    public override RtuOccupation Why => RtuOccupation.DoMeasurementClient;
}