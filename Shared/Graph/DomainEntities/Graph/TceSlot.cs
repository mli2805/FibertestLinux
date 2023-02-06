namespace Fibertest.Graph;

[Serializable]
public class TceSlot
{
    public int Position;
    public int GponInterfaceCount;
    public bool IsPresent => GponInterfaceCount > 0;
}