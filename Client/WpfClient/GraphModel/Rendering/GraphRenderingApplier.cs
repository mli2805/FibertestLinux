using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fibertest.WpfClient
{
    public static class GraphRenderingApplier
    {
        private const int Portion = 50;
        private const int Delay = 20;

        public static async Task ToEmptyGraph(this GraphReadModel graphReadModel, RenderingResult renderingResult)
        {
            var i = 0;
            while (i < renderingResult.NodeVms.Count)
            {
                for (int j = 0; j < Portion; j++)
                {
                    if (i >= renderingResult.NodeVms.Count) break;
                    var nodeVm = renderingResult.NodeVms[i];
                    graphReadModel.Data.Nodes.Add(nodeVm);
                    i++;
                }
                await Task.Delay(Delay);
            }

            i = 0;
            while (i < renderingResult.FiberVms.Count)
            {
                for (int j = 0; j < Portion; j++)
                {
                    if (i >= renderingResult.FiberVms.Count) break;
                    var fiberVm = renderingResult.FiberVms[i];
                    graphReadModel.Data.Fibers.Add(fiberVm);
                    i++;
                }
                await Task.Delay(Delay);
            }
        }

        public static async Task<int> ToExistingGraph(this GraphReadModel graphReadModel, RenderingResult renderingResult)
        {
            var newGoodFibers = new HashSet<Guid>(renderingResult.FiberVms.Select(f => f.Id));
            var fibersToDelete = graphReadModel.Data.Fibers.Where(f => !newGoodFibers.Contains(f.Id)).ToList();
            var i = 0;

            foreach (var fiberVm in fibersToDelete)
            {
                graphReadModel.Data.Fibers.Remove(fiberVm);
                i++;
                if (i == Portion)
                {
                    i = 0;
                    await Task.Delay(Delay);
                }
            }

            var newGoodNodes = new HashSet<Guid>(renderingResult.NodeVms.Select(n => n.Id));
            var nodesToDelete = graphReadModel.Data.Nodes.Where(n => !newGoodNodes.Contains(n.Id)).ToList();

            foreach (var nodeVm in nodesToDelete)
            {
                graphReadModel.Data.Nodes.Remove(nodeVm);
                i++;
                if (i == Portion)
                {
                    i = 0;
                    await Task.Delay(Delay);
                }

            }

            foreach (var nodeVm in renderingResult.NodeVms)
            {
                if (graphReadModel.Data.Nodes.All(n => n.Id != nodeVm.Id))
                {
                    graphReadModel.Data.Nodes.Add(nodeVm);
                    i++;
                    if (i == Portion)
                    {
                        i = 0;
                        await Task.Delay(Delay);
                    }
                }
            }

            foreach (var fiberVm in renderingResult.FiberVms)
            {
                if (graphReadModel.Data.Fibers.All(f => f.Id != fiberVm.Id))
                {
                    graphReadModel.Data.Fibers.Add(fiberVm);
                    i++;
                    if (i == Portion)
                    {
                        i = 0;
                        await Task.Delay(Delay);
                    }

                }
            }

            return renderingResult.NodeVms.Count;
        }
    }
}
