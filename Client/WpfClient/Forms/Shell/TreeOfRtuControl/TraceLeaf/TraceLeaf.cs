using System;
using System.Collections.Generic;
using System.Windows;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Brushes = System.Windows.Media.Brushes;

namespace Fibertest.WpfClient
{
    public class TraceLeaf : Leaf, IPortNumber
    {
        private int _portNumber;
        public int PortNumber
        {
            get => _portNumber;
            set
            {
                if (value == _portNumber) return;
                _portNumber = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IconsVisibility));
                NotifyOfPropertyChange(nameof(LeftMargin));
                NotifyOfPropertyChange(nameof(Name));
                Color = _isInZone ? PortNumber < 1 ? Brushes.Blue : Brushes.Black : Brushes.LightGray;
            }
        }

        public int LeftMargin => PortNumber < 1
            ? 53 // not attached 
            : Parent is RtuLeaf // attached
                ? 53            // RTU  
                : 90;           // BOP

        public Visibility IconsVisibility => IsInZone ? Visibility.Visible : Visibility.Hidden;

        public override string Name
        {
            get
            {
                return PortNumber < 1
                  ? Title
                  : string.Format(Resources.SID_Port_trace, PortNumber, Title);
            }
            set { }
        }

        private BaseRefsSet _baseRefsSet = new BaseRefsSet();
        public BaseRefsSet BaseRefsSet
        {
            get => _baseRefsSet;
            set
            {
                if (Equals(value, _baseRefsSet)) return;
                _baseRefsSet = value;
                NotifyOfPropertyChange();
            }
        }

        private FiberState _traceState;
        public FiberState TraceState
        {
            get => _traceState;
            set
            {
                if (value == _traceState) return;
                _traceState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceStatePictogram));
            }
        }

        private bool _isInZone;
        public bool IsInZone
        {
            get => _isInZone;
            set
            {
                if (value == _isInZone) return;
                _isInZone = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IconsVisibility));
                NotifyOfPropertyChange(nameof(TraceStatePictogram));
                Color = _isInZone
                    ? PortNumber < 1
                        ? FiberState.NotJoined.GetBrush(true)
                        : FiberState.Ok.GetBrush(true)
                    : FiberState.NotInZone.GetBrush(true);
            }
        }

        public Uri TraceStatePictogram => IsInZone
            ? TraceState.GetPictogram()
            : new Uri(@"pack://application:,,,/Resources/LeftPanel/WhiteSquare.png");

       
        private TraceToTceLinkState _traceToTceLinkState;
        public TraceToTceLinkState TraceToTceLinkState
        {
            get => _traceToTceLinkState;
            set
            {
                if (value == _traceToTceLinkState) return;
                _traceToTceLinkState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceTceRelationPictogram));
            }
        }


        public Uri TraceTceRelationPictogram => TraceToTceLinkState.GetPictogram();

        private readonly TraceLeafContextMenuProvider _contextMenuProvider;

        public TraceLeaf(IPortOwner parent, TraceLeafContextMenuProvider contextMenuProvider)
        {
            Parent = (Leaf)parent;
            _contextMenuProvider = contextMenuProvider;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            return _contextMenuProvider.GetMenu(this);
        }
    }
}



