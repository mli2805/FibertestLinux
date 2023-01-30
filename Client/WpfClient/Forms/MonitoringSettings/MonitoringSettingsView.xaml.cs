using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for MonitoringSettingsView.xaml
    /// </summary>
    public partial class MonitoringSettingsView
    {
        private const int GwlStyle = -16;
        private const int WsMinimizebox = 0x20000;
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        public MonitoringSettingsView()
        {
            InitializeComponent();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.B && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                await ((MonitoringSettingsViewModel) DataContext).ReSendBaseRefsForAllSelectedTraces();
            }

        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, GwlStyle) & ~WsMinimizebox);
        }
    }
}
