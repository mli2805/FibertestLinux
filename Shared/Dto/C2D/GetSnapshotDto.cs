namespace Fibertest.Dto
{

    public class GetSnapshotDto : BaseRequest
    {
        public string? ClientIp;
        public int LastIncludedEvent;

        public override string What => "GetSnapshot";
    }


    public class SnapshotParamsDto
    {
        public int PortionsCount;
        public int Size;
    }


    public class SerializedModelDto
    {
        public int PortionsCount;
        public int Size;
        public int LastIncludedEvent;
    }
}