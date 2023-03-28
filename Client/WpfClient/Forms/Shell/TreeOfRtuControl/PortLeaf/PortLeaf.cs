using System.Collections.Generic;
using System.Windows.Media;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class PortLeaf : Leaf, IPortNumber
    {
        private readonly PortLeafContextMenuProvider _contextMenuProvider;
        public int PortNumber { get; set; }

        public override string Name
        {
            get => string.Format(Resources.SID_Port_N, PortNumber);
            set { }
        }

        public int LeftMargin => Parent is OtauLeaf ? 138 : 101;

        public PortLeaf(
            // Specified manually
            Leaf parent, int portNumber, 
            // Resolved with Autofac
            PortLeafContextMenuProvider contextMenuProvider)
        {
            PortNumber = portNumber;
            _contextMenuProvider = contextMenuProvider;
            Parent = parent;
            Color = Brushes.Black;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            return _contextMenuProvider.GetMenu(this);
        }

    }
}