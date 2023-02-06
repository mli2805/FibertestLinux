using Fibertest.Dto;

namespace Fibertest.Graph;

[Serializable]
public class VeexTest
{
    public Guid TestId;
    public Guid TraceId;
    public BaseRefType BasRefType;

    public bool IsOnBop;
    public string? OtauId;

    public DateTime CreationTimestamp;

}