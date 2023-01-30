using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class TracesToEquipmentInjectionViewModel : Screen
    {
        public string Explanation { get; set; }
        public List<CheckBoxModel> Choices { get; set; } // for binding
        public bool ShouldWeContinue { get; set; }
        public bool IsClosed { get; set; }

        private List<Trace> _tracesInNode;
        public TracesToEquipmentInjectionViewModel(List<Trace> tracesInNode)
        {
            _tracesInNode = tracesInNode;
            InitializeChoices();
        }

        private void InitializeChoices()
        {
            Explanation = Resources.SID_Select_traces_which_will_use;
            Choices = new List<CheckBoxModel>();
            foreach (var trace in _tracesInNode)
            {
                var checkBoxModel = new CheckBoxModel() { Id = trace.TraceId, Title = trace.Title, IsChecked = false, IsEnabled = !trace.HasAnyBaseRef};
                Choices.Add(checkBoxModel);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Select_traces_for_equipment;
        }

        public List<Guid> GetChosenTraces()
        {
            return (from checkBoxModel in Choices where checkBoxModel.IsChecked select checkBoxModel.Id).ToList();
        }

        public void Accept()
        {
            ShouldWeContinue = true;
            CloseView();
        }

        public void Cancel()
        {
            ShouldWeContinue = false;
            CloseView();
        }

        private async void CloseView()
        {
            IsClosed = true;
            await TryCloseAsync();
        }

    }
}
