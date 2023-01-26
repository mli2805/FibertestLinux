using Fibertest.Dto;

namespace Fibertest.DataCenter
{
    public class TrapParserResult
    {
        public Guid TceId;
        public int Slot;
        public int GponInterface;
        public FiberState State;
        public string ZteEventId = String.Empty;
    }
}