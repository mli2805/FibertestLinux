using System.Collections.Generic;
using System.Linq;

namespace Fibertest.WpfClient
{
    public static class TreeModelExt
    {
        public static List<TraceLeaf> GetAttachedTraces(this RtuLeaf rtuLeaf)
        {
            var list = GetAttachedTracesOneLevel(rtuLeaf).ToList();
            list.AddRange(rtuLeaf
                .ChildrenImpresario
                .Children
                .OfType<OtauLeaf>()
                .SelectMany(GetAttachedTracesOneLevel));
            return list;
        }

        private static IEnumerable<TraceLeaf> GetAttachedTracesOneLevel(IPortOwner portOwner)
        {
            return portOwner.ChildrenImpresario
                .Children
                .OfType<TraceLeaf>()
                .Where(t => t.PortNumber > 0);
        }
    }
}