namespace Fibertest.Dto;

public class SerializedModelDto : RequestAnswer
{
    public int PortionsCount;
    public int Size;
    public int LastIncludedEvent;
}

public class SerializedModelPortionDto : RequestAnswer
{
    public byte[] Bytes = null!;
}