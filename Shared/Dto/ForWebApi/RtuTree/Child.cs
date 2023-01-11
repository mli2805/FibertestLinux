namespace Fibertest.Dto;

public class ChildDto
{
    public int Port;
    public ChildType ChildType;

    public ChildDto(ChildType childType)
    {
        ChildType = childType;
    }
}