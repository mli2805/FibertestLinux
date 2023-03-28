using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class PathFinder
    {
        private readonly Model _model;

        public PathFinder(Model model)
        {
            _model = model;
        }

        private IEnumerable<Guid> GetAdjacentNodes(Guid nodeId)
        {
            foreach (var fiber in _model.Fibers)
            {
                if (fiber.NodeId1 == nodeId)
                    yield return fiber.NodeId2;
                if (fiber.NodeId2 == nodeId)
                    yield return fiber.NodeId1;
            }
        }

        private void FindPathRecursive(Guid end, List<Guid> path)
        {
            var previous = path.Last();

            foreach (var nodeId in GetAdjacentNodes(previous))
            {
                if (nodeId == end)
                {
                    path.Add(end);
                    return;
                }

                if (path.Contains(nodeId))
                    continue;

                path.Add(nodeId);
                FindPathRecursive(end, path);

                if (path.Last() != end)
                    path.Remove(nodeId);
                else return;
            }
        }

        public async Task<List<Guid>?> FindPath(Guid start, Guid end)
        {
            await Task.Delay(1);
            var path = new List<Guid> {start};

            FindPathRecursive(end, path);
            if (path.Last() != end)
                path = null;
            return path;
        }
    }
}