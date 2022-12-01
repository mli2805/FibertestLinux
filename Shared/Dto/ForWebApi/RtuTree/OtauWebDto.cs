namespace Fibertest.Dto
{
    public class OtauWebDto : ChildDto
    {
        public Guid OtauId;
        public Guid RtuId;
        public NetAddress? OtauNetAddress;
        public bool IsOk;
        public string? Serial;

        public List<ChildDto> Children = new List<ChildDto>();

        public OtauWebDto(ChildType childType) : base(childType)
        {
        }
    }
}