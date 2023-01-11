namespace Fibertest.Dto;

public class AssignBaseRefDtoWithFiles
{
    public Guid RtuId;
    public RtuMaker RtuMaker;
    public string? OtdrId; //  in VeEX RTU main OTDR has its own ID
    
    public Guid TraceId;
    
    public OtauPortDto? OtauPortDto; // could be null if trace isn't attached to port yet
    
    public List<BaseRefFile>? BaseRefs;

}