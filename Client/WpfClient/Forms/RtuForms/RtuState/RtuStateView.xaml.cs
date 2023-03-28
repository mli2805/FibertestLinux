using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for RtuStateView.xaml
    /// </summary>
    public partial class RtuStateView
    {
        private const int GwlStyle = -16;
        private const int WsMinMax = 0x30000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        public RtuStateView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, GwlStyle) & ~WsMinMax);
        }
    }
}
