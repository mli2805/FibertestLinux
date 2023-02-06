using Fibertest.Dto;

namespace Fibertest.Graph;

[Serializable]
public class Otau
{
    public Guid Id;
    public Guid RtuId;
    public string? VeexRtuMainOtauId;
    public bool IsMainOtau;

    public NetAddress NetAddress = new();
    public string? Serial;
    public int PortCount;

    public int MasterPort;
    public bool IsOk;
}