using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autofac;
using Fibertest.Dto;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for MapUserControl.xaml
    /// </summary>
    public partial class MapUserControl : INotifyPropertyChanged
    {
        public GraphReadModel GraphReadModel => (GraphReadModel)DataContext;

        public MapUserControl()
        {
            InitializeComponent();
            DataContextChanged += MapUserControl_DataContextChanged;
            // map events
            MainMap.MouseEnter += MainMap_MouseEnter;
            MainMap.OnTraceDefiningCancelled += MainMap_OnTraceDefiningCancelled;
            MainMap.Loaded += MainMap_Loaded;
            MainMap.PropertyChanged += MainMap_PropertyChanged;
        }

        private async void MainMap_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Limits")
                await GraphReadModel.RefreshVisiblePart();
        }

        private void MainMap_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing += Window_Closing;
                MainMap.EvaluateMapLimits(window.Height, window.Width);
            }
        }

        private void MainMap_OnTraceDefiningCancelled()
        {
            SetBanner("");
        }

        private void ConfigureMap()
        {
            // var accessMode = GraphReadModel.Config.Value.Map.MapAccessMode;
            // MainMap.Manager.Mode = AccessModeExt.FromEnumConstant(accessMode);
            MainMap.Manager.Mode = GraphReadModel.Config.Value.Map.MapAccessMode;

            var maxZoom = GraphReadModel.Config.Value.Map.MaxZoom;
            MainMap.MaxZoom = maxZoom;

            if (GraphReadModel.IsInGisVisibleMode)
            {
                var provider = GraphReadModel.Config.Value.Map.GMapProvider;
                switch (provider)
                {
                    case "OpenStreetMap":
                        {
                            MainMap.MapProvider = GMapProviders.OpenStreetMap;
                            GMapProvider.Language = LanguageType.English;
                            break;
                        }
                    case "GoogleMap":
                        {
                            MainMap.MapProvider = GMapProviders.GoogleMap;
                            GMapProvider.Language = LanguageType.English;
                            break;
                        }
                    case "GoogleSatelliteMap":
                        {
                            MainMap.MapProvider = GMapProviders.GoogleSatelliteMap;
                            GMapProvider.Language = LanguageType.English;
                            break;
                        }
                    case "GoogleHybridMap":
                        {
                            MainMap.MapProvider = GMapProviders.GoogleHybridMap;
                            GMapProvider.Language = LanguageType.English;
                            break;
                        }
                    default:
                        MainMap.MapProvider = GMapProviders.EmptyProvider;
                        break;
                }
            }
            else
            {
                MainMap.MapProvider = GMapProviders.EmptyProvider;
            }
        }

        private void MapUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            var graph = (GraphReadModel)e.NewValue;
            graph.MainMap = MainMap;

            ConfigureMap();

            MainMap.CurrentGis = GraphReadModel.CurrentGis;
            MainMap.IsInGisVisibleMode = GraphReadModel.IsInGisVisibleMode;

            graph.Data.Nodes.CollectionChanged += NodesCollectionChanged;
            graph.Data.Fibers.CollectionChanged += FibersCollectionChanged;

            graph.PropertyChanged += Graph_PropertyChanged;

            ApplyAddedNodes(graph.Data.Nodes);
            ApplyAddedFibers(graph.Data.Fibers);
        }

        private void Graph_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GraphReadModel.SelectedGraphVisibilityItem))
                ChangeVisibility(GraphReadModel.SelectedGraphVisibilityItem.Level);
            if (e.PropertyName == nameof(GraphReadModel.IsInGisVisibleMode))
                ConfigureMap();
        }

        public void ChangeVisibility(GraphVisibilityLevel selectedLevel)
        {
            foreach (var marker in MainMap.Markers)
            {
                if (marker is GMapRoute gMapRoute)
                {
                    gMapRoute.Shape.Visibility = selectedLevel >= GraphVisibilityLevel.RtuAndTraces
                        ? Visibility.Visible
                        : Visibility.Hidden;
                }
                else
                {
                    // GMapMarker - node
                    if (marker.IsHighlighting) continue;

                    marker.Shape.Visibility =
                        selectedLevel >= ((MarkerControl)marker.Shape).EqType.GetEnabledVisibilityLevel()
                            ? Visibility.Visible
                            : Visibility.Hidden;
                }
            }
        }


        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        public void SetBanner(string message)
        {
            GraphReadModel.CommonStatusBarViewModel.StatusBarMessage2 = message;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = GraphReadModel.Config.Value.Map.Zoom;
            MainMap.CurrentGis.SetMargin((int)MainMap.Zoom);
            var lat = GraphReadModel.Config.Value.Map.CenterLatitude;
            var lng = GraphReadModel.Config.Value.Map.CenterLongitude;
            MainMap.Position = new PointLatLng(lat, lng);

            MainMap.ContextMenu =
                GraphReadModel.GlobalScope.Resolve<MapContextMenuProvider>().GetMapContextMenu();
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            var saveZoomLimit = GraphReadModel.Config.Value.Map.SaveMaxZoomNoMoreThan;
            GraphReadModel.Config.Update(c=>c.Map.Zoom = MainMap.Zoom > saveZoomLimit ? saveZoomLimit : (int)MainMap.Zoom);
            GraphReadModel.Config.Update(c=>c.Map.CenterLatitude = MainMap.Position.Lat);
            GraphReadModel.Config.Update(c=>c.Map.CenterLongitude = MainMap.Position.Lng);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!MainMap.IsInDistanceMeasurementMode)
                {
                    SetBanner(StringResources.Resources.SID_Distance_measurement_mode);
                    MainMap.IsInDistanceMeasurementMode = true;
                    MainMap.StartNode = null;
                }
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && MainMap.IsInDistanceMeasurementMode)
            {
                SetBanner("");
            }
        }
    }
}