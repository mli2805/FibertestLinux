using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class TraceChoiceViewModel : Screen
    {
        public List<Trace> Rows { get; set; }
        public Trace SelectedTrace { get; set; }
        public bool IsAnswerPositive;

        public void Initialize(List<Trace> traces)
        {
            IsAnswerPositive = false;
            Rows = traces;
            SelectedTrace = Rows.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Select_trace;
        }

        public async void Ok()
        {
            IsAnswerPositive = true;
            await TryCloseAsync();
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }

    }
}
