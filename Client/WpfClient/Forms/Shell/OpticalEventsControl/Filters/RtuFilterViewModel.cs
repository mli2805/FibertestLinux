using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class RtuFilterViewModel : Screen
    {
        private readonly Model _readModel;
        private RtuGuidFilter _selectedRow;
        public List<RtuGuidFilter> Rows { get; set; }

        public RtuGuidFilter SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }



        public RtuFilterViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Select_RTU;
        }

        public void Initialize()
        {
            Rows = new List<RtuGuidFilter> ();
            foreach (var rtu in _readModel.Rtus)
            {
                if (!string.IsNullOrEmpty(rtu.Title))
                    Rows.Add(new RtuGuidFilter(rtu.Id, rtu.Title));
            }
            Rows.Add(new RtuGuidFilter());

            SelectedRow = Rows.First();
        }

        public async void Apply()
        {
            await TryCloseAsync(true);
        }
    }
}
