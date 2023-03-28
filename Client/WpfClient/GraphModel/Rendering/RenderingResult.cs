using System.Collections.Generic;

namespace Fibertest.WpfClient
{
    public class RenderingResult
    {
        public readonly List<NodeVm> NodeVms = new List<NodeVm>();
        public readonly List<FiberVm> FiberVms = new List<FiberVm>();
    }
}