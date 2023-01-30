using System.Collections.Generic;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class OtdrParametersThroughServerSetterViewModel : Screen
    {
        private readonly IWritableConfig<OtdrParametersConfig> _config;
        public bool IsAnswerPositive { get; set; }

        public OtdrParametersViewModel OtdrParametersViewModel { get; set; } = new OtdrParametersViewModel();

        public OtdrParametersThroughServerSetterViewModel(IWritableConfig<OtdrParametersConfig> config)
        {
            _config = config;
        }

        public void Initialize(TreeOfAcceptableMeasParams treeOfAcceptableMeasParams)
        {
            OtdrParametersViewModel.Initialize(treeOfAcceptableMeasParams, _config);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement__Client_;
        }

        public async void Measure()
        {
            IsAnswerPositive = true;
            await TryCloseAsync();
        }

        public List<MeasParamByPosition> GetSelectedParameters()
        {
            return OtdrParametersViewModel.GetSelectedParameters();
        }

        public VeexMeasOtdrParameters GetVeexSelectedParameters()
        {
            return OtdrParametersViewModel.GetVeexSelectedParameters(false);
        }

        public async void Close()
        {
            IsAnswerPositive = false;
            await TryCloseAsync();
        }
    }
}