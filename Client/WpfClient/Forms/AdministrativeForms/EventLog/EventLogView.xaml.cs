using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for EventLogView.xaml
    /// </summary>
    public partial class EventLogView 
    {
        private const int GwlStyle = -16;
        private const int WsMinimize = 0x20000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        public EventLogView()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, GwlStyle) & ~WsMinimize);
        }
    }
}
