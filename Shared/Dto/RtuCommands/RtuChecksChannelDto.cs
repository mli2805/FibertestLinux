namespace Fibertest.Dto;

public class RtuChecksChannelDto
{
    public Guid RtuId;
    public string Version;
    public bool IsMainChannel;

    public RtuChecksChannelDto(Guid rtuId, string version, bool isMainChannel)
    {
        RtuId = rtuId;
        Version = version;
        IsMainChannel = isMainChannel;
    }
}