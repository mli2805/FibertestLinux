namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for FiberWithNodesAddView.xaml
    /// </summary>
    public partial class FiberWithNodesAddView
    {
        public FiberWithNodesAddView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Count.Focus();
            Count.Select(0, Count.Text.Length);
        }
    }
}
