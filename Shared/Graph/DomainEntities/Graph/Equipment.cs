using Fibertest.Dto;

namespace Fibertest.Graph;

[Serializable]
public class Equipment
{
    public Guid EquipmentId;
    public Guid NodeId;
    public string? Title;
    public EquipmentType Type; 
    public int CableReserveLeft;
    public int CableReserveRight;
    public string? Comment;

}