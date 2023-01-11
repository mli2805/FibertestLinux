namespace Fibertest.Dto;

public class BaseRefDto
{
    public Guid Id;
    public BaseRefType BaseRefType;
    public string? UserName;
    public DateTime SaveTimestamp;
    public TimeSpan Duration;
    public int SorFileId;
    public byte[]? SorBytes;
}