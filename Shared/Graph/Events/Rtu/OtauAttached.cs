using Fibertest.Dto;

namespace Fibertest.Graph;

public class OtauAttached
{
    public Guid Id;
    public Guid RtuId;

    public NetAddress NetAddress = new();
    public string? Serial;
    public int PortCount;

    public int MasterPort;
    public bool IsOk;
}