namespace Fibertest.Graph
{
    
    public class TceSlot
    {
        public int Position;
        public int GponInterfaceCount;
        public bool IsPresent => GponInterfaceCount > 0;
    }
}