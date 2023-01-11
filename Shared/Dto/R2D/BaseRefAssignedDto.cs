namespace Fibertest.Dto;

public class BaseRefAssignedDto : RequestAnswer
{
    public BaseRefType BaseRefType; // type of base ref where error happened
        
    public int Landmarks;
    public int Nodes;
    public int Equipments;
    public string WaveLength = string.Empty;
        
    public List<VeexTestCreatedDto> AddVeexTests = new List<VeexTestCreatedDto>();
    public List<Guid> RemoveVeexTests = new List<Guid>();

    public BaseRefAssignedDto() {}

    public BaseRefAssignedDto(ReturnCode returnCode) : base(returnCode)
    {
    }
}