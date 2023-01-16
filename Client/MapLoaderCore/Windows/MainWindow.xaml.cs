using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            MainMap.CanDragMap = true;
            MainMap.MaxZoom = 24;
            MaxZoom.Text = "17";

            // set cache mode only if no internet avaible
            if (!PingNetwork("google.com"))
            {
                MainMap.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("Нет соединения с сетью интернет, переходим в режим CacheOnly.",
                    "MapLoader", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // config map
            MainMap.MapProvider = GMapProviders.OpenStreetMap;
            //  MainMap.Zoom = 2;

            // map events
            MainMap.OnTileLoadComplete += MainMap_OnTileLoadComplete;
            MainMap.OnTileLoadStart += MainMap_OnTileLoadStart;
            MainMap.MouseEnter += MainMap_MouseEnter;

            GroupBox4.Header = "готово";
            ProgressBar2.Visibility = Visibility.Hidden;
            MainMap.Manager.OnTileCacheStart += MainMap_OnTileCacheStart;
            MainMap.Manager.OnTileCacheComplete += MainMap_OnTileCacheComplete;

            // get map types
            var providers = new List<GMapProvider>
            {
                OpenStreetMapProvider.Instance,
                GoogleMapProvider.Instance,
                GoogleSatelliteMapProvider.Instance,
                GoogleHybridMapProvider.Instance,
            };
            ComboBoxMapType.ItemsSource = providers;

            //   ComboBoxMapType.ItemsSource = GMapProviders.List;
            ComboBoxMapType.DisplayMemberPath = "Name";
            ComboBoxMapType.SelectedItem = MainMap.MapProvider;

            // access mode
            ComboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
            ComboBoxMode.SelectedItem = MainMap.Manager.Mode;
        }

        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        // tile loading starts
        void MainMap_OnTileLoadStart()
        {
            System.Windows.Forms.MethodInvoker m = delegate
            {
                GroupBox3.Header = "";
                ProgressBar1.Visibility = Visibility.Visible;
            };

            try
            {
                Dispatcher?.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void MainMap_OnTileCacheStart()
        {
            System.Windows.Forms.MethodInvoker m = delegate
            {
                GroupBox4.Header = "сохранение на диске";
                ProgressBar2.Visibility = Visibility.Visible;
            };

            try
            {
                Dispatcher?.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void MainMap_OnTileCacheComplete()
        {
            System.Windows.Forms.MethodInvoker m = delegate
            {
                GroupBox4.Header = "готово";
                ProgressBar2.Visibility = Visibility.Hidden;
            };

            try
            {
                Dispatcher?.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // tile loading stops
        void MainMap_OnTileLoadComplete(long elapsedMilliseconds)
        {
            MainMap.ElapsedMilliseconds = elapsedMilliseconds;

            System.Windows.Forms.MethodInvoker m = delegate
            {
                ProgressBar1.Visibility = Visibility.Hidden;
                GroupBox3.Header = "загрузка длилась " + MainMap.ElapsedMilliseconds + "мс";
            };

            try
            {
                Dispatcher?.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // prefetch
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            RectLatLng area = MainMap.SelectedArea;
            if (!area.IsEmpty)
            {
                var upToZoom = int.Parse(MaxZoom.Text);
                var messageBoxText = $"Загрузить данные выделенного прямоугольника\n для уровней с {(int)MainMap.Zoom} по {upToZoom} ?";
                //  var messageBoxText = $"Download data for area from Zoom = {(int)MainMap.Zoom} up to {upToZoom} ?";
                MessageBoxResult res = MessageBox.Show(messageBoxText, "Map loader", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No) return;

                for (int i = (int)MainMap.Zoom; i <= upToZoom; i++)
                {
                    TilePrefetcher obj = new TilePrefetcher();
                    obj.Owner = this;
                    obj.ShowCompleteMessage = false;
                    obj.Start(area, i, MainMap.MapProvider, 100);
                }

                MessageBox.Show("Данные загружены успешно!");
            }
            else
            {
                MessageBox.Show("Выделите область карты для загрузки\n (Shift + левая кнопка мыши)", "Map loader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // access mode
        private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
        {
            MainMap.Manager.Mode = (AccessMode)ComboBoxMode.SelectedItem;
            MainMap.ReloadMap();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                MainMap.Bearing--;
            }
            else if (e.Key == Key.Z)
            {
                MainMap.Bearing++;
            }
        }

        public static bool PingNetwork(string hostNameOrAddress)
        {
            bool pingStatus;

            using (Ping p = new Ping())
            {
                byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                int timeout = 4444; // 4s

                try
                {
                    PingReply reply = p.Send(hostNameOrAddress, timeout, buffer);
                    pingStatus = (reply?.Status == IPStatus.Success);
                }
                catch (Exception)
                {
                    pingStatus = false;
                }
            }

            return pingStatus;
        }
    }


}
