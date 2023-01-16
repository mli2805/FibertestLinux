using System.Windows.Controls;

namespace MapLoaderCore
{
    /// <summary>
    ///     Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : UserControl
    {
        public Test(string txt)
        {
            InitializeComponent();

            Text.Text = txt;
        }
    }
}
