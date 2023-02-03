using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Fibertest.WpfClient
{
    /// <summary>
    /// Interaction logic for OpticalEventsView.xaml
    /// </summary>
    public partial class OpticalEventsView
    {
       
        public OpticalEventsView()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            Selector? selector = sender as Selector;
            if ( selector is DataGrid dataGrid && selector.SelectedItem != null && dataGrid.SelectedIndex >= 0 )
            {
                try
                {
                    dataGrid.ScrollIntoView( selector.SelectedItem );
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}
