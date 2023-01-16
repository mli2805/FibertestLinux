using System.Windows.Controls;
using GMap.NET;

namespace MapLoaderCore
{
    /// <summary>
    ///     Interaction logic for Circle.xaml
    /// </summary>
    public partial class Circle : UserControl
    {
        public Circle()
        {
            InitializeComponent();
        }

        public PointLatLng Center;
        public PointLatLng Bound;
    }
}
