using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.WpfClient
{
    public class OtdrParametersTemplatesViewModel : PropertyChangedBase
    {
        private readonly IWritableConfig<ClientConfig> _config;
        public OtdrParametersTemplateModel Model { get; set; } = new OtdrParametersTemplateModel();
        private Rtu _rtu = null!;

        public Visibility ListBoxVisibility { get; set; }
        public Visibility NoOptionsLineVisibility { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public OtdrParametersTemplatesViewModel(IWritableConfig<ClientConfig> config)
        {
            _config = config;
        }

        public void Initialize(Rtu rtu, bool isForRtu)
        {
            _rtu = rtu;
            Model.Initialize(rtu);
            ListBoxVisibility = isForRtu ? Visibility.Collapsed : Visibility.Visible;
            NoOptionsLineVisibility = isForRtu ? Visibility.Visible : Visibility.Collapsed;

            var opUnit = _config.Value.OtdrParameters.OpUnit;
            Model.Units = rtu.AcceptableMeasParams.Units.Keys.ToList();
            Model.SelectedUnit = Model.Units.Count > opUnit ? Model.Units[opUnit] : Model.Units.First();
            var branchOfAcceptableMeasParams = rtu.AcceptableMeasParams.Units[Model.SelectedUnit];
            Model.BackScatteredCoefficient = branchOfAcceptableMeasParams.BackscatteredCoefficient;
            Model.RefractiveIndex = branchOfAcceptableMeasParams.RefractiveIndex;
        }

        public bool IsAutoLmaxSelected()
        {
            return Model.SelectedOtdrParametersTemplate.Id == 0;
        }

        public List<MeasParamByPosition> GetSelectedParameters()
        {
            var branch = _rtu.AcceptableMeasParams.Units[Model.SelectedUnit];

            var result = new List<MeasParamByPosition>
            {
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.Unit, Position = Model.Units.IndexOf(Model.SelectedUnit)},
                new MeasParamByPosition
                    {Param = ServiceFunctionFirstParam.Bc, Position = (int) (Model.BackScatteredCoefficient * 100)},
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.Ri, Position = (int) (Model.RefractiveIndex * 100000)},
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.IsTime, Position = 1},
            };

            if (Model.SelectedOtdrParametersTemplate.Id != 0)
            {
                var leaf = branch.Distances[Model.SelectedOtdrParametersTemplate.Lmax];
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Lmax,
                    Position = branch.Distances.Keys.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Lmax)
                });
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Res,
                    Position = leaf.Resolutions.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Dl)
                });
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Pulse,
                    Position = leaf.PulseDurations.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Tp)
                });
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Time,
                    Position = leaf.PeriodsToAverage.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Time)
                });
            }

            return result;
        }

       

      
        public VeexMeasOtdrParameters GetVeexSelectedParameters()
        {
            var result = Model.GetVeexMeasOtdrParametersBase(false);

            if (Model.SelectedOtdrParametersTemplate.Id != 0)
            {
                result.distanceRange = Model.SelectedOtdrParametersTemplate.Lmax;
                result.resolution = Model.SelectedOtdrParametersTemplate.Dl;
                result.pulseDuration = Model.SelectedOtdrParametersTemplate.Tp;
                result.averagingTime = Model.SelectedOtdrParametersTemplate.Time;
            }

            return result;
        }

    }
   
}
