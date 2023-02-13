using System.Collections.ObjectModel;

namespace Fibertest.WpfClient
{
    public class GraphReadModelData
    {
        public ObservableCollection<NodeVm> Nodes { get; set; } = null!;
        public ObservableCollection<FiberVm> Fibers { get; set; } = null!;
    }
}