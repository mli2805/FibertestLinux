using Fibertest.Dto;

namespace Fibertest.Graph;

[Serializable]
public class GponPortRelation
{
    // public Guid Id;
    public Guid TceId;
    public int SlotPosition;
    public int GponInterface;
    public Guid RtuId;
    public RtuMaker RtuMaker;
    public OtauPortDto? OtauPortDto;
    public Guid TraceId;
}