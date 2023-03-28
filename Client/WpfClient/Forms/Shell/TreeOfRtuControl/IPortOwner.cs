using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public interface IPortOwner
    {
        int OwnPortCount { get; set; }
        ChildrenImpresario ChildrenImpresario { get; }
        NetAddress? OtauNetAddress { get; set; }
        string? Serial { get; set; }
    }
}