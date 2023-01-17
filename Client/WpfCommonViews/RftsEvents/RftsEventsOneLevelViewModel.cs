using System;
using System.Data;
using System.Linq;
using Fibertest.Graph;
using Fibertest.OtdrDataFormat;
using Fibertest.StringResources;
using Fibertest.Utils;

namespace WpfCommonViews
{
    public class RftsEventsOneLevelViewModel
    {
        public DataTable BindableTable { get; set; } = null!;
        public OneLevelTableContent OneLevelTableContent { get; set; } = null!;

        public bool IsFailed { get; set; }

        public RftsEventsOneLevelEeltViewModel EeltViewModel { get; set; } = null!;

        public RftsEventsOneLevelViewModel(OtdrDataKnownBlocks sorData, RftsEventsBlock? rftsEventsBlock, RftsLevel rftsLevel)
        {
            if (rftsEventsBlock == null)
            {
                return;
            }

            try
            {
                OneLevelTableContent = new SorDataToViewContent(sorData, rftsEventsBlock).Parse();
            }
            catch (Exception )
            {
                return;
            }

            CreateTable(OneLevelTableContent.Table.First().Value.Length-1,
                    rftsLevel.LevelName.ConvertToFiberState().ToLocalizedString());
            PopulateTable();
            EeltViewModel = new RftsEventsOneLevelEeltViewModel(sorData.KeyEvents.EndToEndLoss, rftsLevel.EELT, rftsEventsBlock.EELD);
            IsFailed = OneLevelTableContent.IsFailed || EeltViewModel.IsFailed;
        }

        private void CreateTable(int eventCount, string header)
        {
            BindableTable = new DataTable();
            BindableTable.TableName = header;
            BindableTable.Columns.Add(new DataColumn(Resources.SID_Parameters));
            for (int i = 0; i < eventCount; i++)
                BindableTable.Columns.Add(new DataColumn(string.Format(Resources.SID_Event_N_0_, i)) { DataType = typeof(string) });
        }

        private void PopulateTable()
        {
            foreach (var pair in OneLevelTableContent.Table)
            {
                DataRow newRow = BindableTable.NewRow();
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    // if (pair.Key > 300 && pair.Key < 500 && i == 1)
                    //     newRow[i] = "";
                    // else
                        newRow[i] = pair.Value[i];
                }
                BindableTable.Rows.Add(newRow);
            }
        }

        public string GetState()
        {
            return IsFailed
                ? string.Format(Resources.SID_fail___0__km_, OneLevelTableContent.FirstProblemLocation)
                : Resources.SID_pass;
        }
    }
}
