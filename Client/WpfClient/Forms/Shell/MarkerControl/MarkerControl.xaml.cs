using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Autofac;
using Fibertest.Dto;
using Fibertest.Graph;
using GMap.NET.WindowsPresentation;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for MarkerControl.xaml
    /// </summary>
    public partial class MarkerControl 
    {
        private Popup _popup;
        private Label _label;
        public readonly GMapMarker GMapMarker;
        private readonly ILifetimeScope _globalScope;

        public EquipmentType Type
        {
            get => EqType;
            set
            {
                EqType = value;
                AssignBitmapImageTo(Icon);
            }
        }

        public FiberState State { get; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                InitializePopup();
            }
        }

        public readonly Map MainMap;
        public readonly MapUserControl Owner;
        public EquipmentType EqType;
        private string _title;

        public new ContextMenu ContextMenu { get; set; }

        public MarkerControl(MapUserControl owner, GMapMarker gMapMarker, EquipmentType type, FiberState state, string title, ILifetimeScope globalScope)
        {
            InitializeComponent();
            Owner = owner;
            MainMap = owner.MainMap;
            GMapMarker = gMapMarker;
            _globalScope = globalScope;
            Type = type;
            State = state;
            Title = title;

            Subscribe();
            InitializePopup();
        }

        private void Subscribe()
        {
            Unloaded += MarkerControl_Unloaded;
            Loaded += MarkerControl_Loaded;
            SizeChanged += MarkerControl_SizeChanged;
            MouseEnter += MarkerControl_MouseEnter;
            MouseLeave += MarkerControl_MouseLeave;
            MouseWheel += MarkerControl_MouseWheel;
            PreviewMouseMove += MarkerControl_PreviewMouseMove;
            PreviewMouseLeftButtonUp += MarkerControl_PreviewMouseLeftButtonUp;
            PreviewMouseLeftButtonDown += MarkerControl_PreviewMouseLeftButtonDown;
            PreviewMouseRightButtonUp += MarkerControl_PreviewMouseRightButtonUp;
        }

     

        private void InitializePopup()
        {
            _label = new Label { Background = Brushes.White, Foreground = Brushes.Black, FontSize = 14, Content = Title, };
            _popup = new Popup { Placement = PlacementMode.Mouse, Child = _label, };
        }

        private void AssignBitmapImageTo(Image destination)
        {
            destination.Width = Type == EquipmentType.Rtu ? 40 : Type == EquipmentType.AccidentPlace ? 24 : 8;
            destination.Height = Type == EquipmentType.Rtu ? 28 : Type == EquipmentType.AccidentPlace ? 24 : 8;
            
            destination.Source = EquipmentTypeWpfExt.GetNodePictogram(Type, State);
            destination.ContextMenu = ContextMenu;
        }



        void MarkerControl_Loaded(object sender, RoutedEventArgs e)
        {
            AssignBitmapImageTo(Icon);
            if (Icon.Source.CanFreeze)
                Icon.Source.Freeze();
            Icon.Visibility = Visibility.Visible;
        }

        void MarkerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= MarkerControl_Unloaded;
            Loaded -= MarkerControl_Loaded;
            SizeChanged -= MarkerControl_SizeChanged;
            MouseEnter -= MarkerControl_MouseEnter;
            MouseLeave -= MarkerControl_MouseLeave;
            MouseWheel -= MarkerControl_MouseWheel;
            PreviewMouseMove -= MarkerControl_PreviewMouseMove;
            PreviewMouseLeftButtonUp -= MarkerControl_PreviewMouseLeftButtonUp;
            PreviewMouseLeftButtonDown -= MarkerControl_PreviewMouseLeftButtonDown;
            PreviewMouseRightButtonUp -= MarkerControl_PreviewMouseRightButtonUp;

            GMapMarker.Shape = null;
            Icon.Source = null;
            Icon = null;
            _popup = null;
            _label = null;
        }

        private void OpenNodeContextMenu()
        {
            var menuProvider = _globalScope.Resolve<NodeVmContextMenuProvider>();
            ContextMenu = menuProvider.GetNodeContextMenu(this);
            ContextMenu.IsOpen = true;
        }
        private void OpenRtuContextMenu()
        {
            var menuProvider = _globalScope.Resolve<RtuVmContextMenuProvider>();
            ContextMenu = menuProvider.GetRtuContextMenu(this);
            ContextMenu.IsOpen = true;
        }

    }
}
