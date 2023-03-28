using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Fibertest.WpfCommonViews
{
    /// <summary>
    /// Interaction logic for RftsEventsOneLevelView.xaml
    /// </summary>
    public partial class RftsEventsOneLevelView
    {
        public RftsEventsOneLevelView()
        {
            InitializeComponent();
        }

        private void EventsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow row = e.Row;
            if (!(row.Item is DataRowView rowView))
                return;
            var name = (string?)rowView.Row.ItemArray[0] ?? "";
            if (name.StartsWith(@" "))
            {
                row.FontWeight = FontWeights.Bold;
                row.Background = Brushes.LightCyan;
            }

            Dispatcher.InvokeAsync(ColorFailedEvents, DispatcherPriority.ApplicationIdle);
        }

        private void ColorFailedEvents()
        {
            var row = GetRow(3); // row with the event states
            if (!(row.Item is DataRowView rowView))
                return;
            var cc = rowView.Row.ItemArray.Length;
            for (int i = 1; i < cc; i++)
            {
                var text = (string?) rowView.Row.ItemArray[i];
                if (text != StringResources.Resources.SID_pass && text != "")
                {
                    DataGridCell? cell = GetCell(row, i);
                    if (cell != null)
                        cell.Background = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private DataGridCell? GetCell(DataGridRow? row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter? presenter = GetVisualChild<DataGridCellsPresenter>(row);
                
                if (presenter == null)
                {
                    EventsDataGrid.ScrollIntoView(row, EventsDataGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter!.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        private DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)EventsDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            // if (row == null)
            // {
            //     // May be virtualized, bring into view and try again.
            //     EventsDataGrid.UpdateLayout();
            //     EventsDataGrid.ScrollIntoView(EventsDataGrid.Items[index]);
            //     row = (DataGridRow)EventsDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            // }
            return row;
        }

        private static T? GetVisualChild<T>(Visual parent) where T : Visual
        {
            T? child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
