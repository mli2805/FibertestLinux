using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.WpfCommonViews
{
    public class OtdrParametersViewModel : PropertyChangedBase
    {
        private TreeOfAcceptableMeasParams _treeOfAcceptableMeasParams = null!;
        private IWritableOptions<OtdrParametersConfig> _config = null!;
        public OtdrParametersModel Model { get; set; } = null!;

        public void Initialize(TreeOfAcceptableMeasParams treeOfAcceptableMeasParams, IWritableOptions<OtdrParametersConfig> config)
        {
            _treeOfAcceptableMeasParams = treeOfAcceptableMeasParams;
            _config = config;
            Model = new OtdrParametersModel();
            InitializeControls();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedUnit":
                    ReInitializeForSelectedUnit();
                    break;
                case "SelectedDistance":
                    ReInitializeForSelectedDistance();
                    break;
            }
        }

        private void InitializeControls()
        {
            var opUnit = _config.Value.OpUnit;
            Model.Units = _treeOfAcceptableMeasParams.Units.Keys.ToList();
            Model.SelectedUnit = Model.Units.Count > opUnit ? Model.Units[opUnit] : Model.Units.First();

            var branchOfAcceptableMeasParams = _treeOfAcceptableMeasParams.Units[Model.SelectedUnit];
            Model.BackscatteredCoefficient = branchOfAcceptableMeasParams.BackscatteredCoefficient;
            Model.RefractiveIndex = branchOfAcceptableMeasParams.RefractiveIndex;

            var opDistance = _config.Value.OpDistance;
            Model.Distances = branchOfAcceptableMeasParams.Distances
                .Keys.OrderBy(x => double.Parse(x, new CultureInfo(@"en-US"))).ToList();
            Model.SelectedDistance = Model.Distances.Count > opDistance ? Model.Distances[opDistance] : Model.Distances.First();

            var leafOfAcceptableMeasParams = branchOfAcceptableMeasParams.Distances[Model.SelectedDistance];
            var opResolution = _config.Value.OpResolution;
            Model.Resolutions = leafOfAcceptableMeasParams.Resolutions.ToList();
            Model.SelectedResolution = Model.Resolutions.Count > opResolution ? Model.Resolutions[opResolution] : Model.Resolutions.First();

            var opPulseDuration = _config.Value.OpPulseDuration;
            Model.PulseDurations = leafOfAcceptableMeasParams.PulseDurations.ToList();
            Model.SelectedPulseDuration = Model.PulseDurations.Count > opPulseDuration 
                ? Model.PulseDurations[opPulseDuration] : Model.PulseDurations.First();

            var opMeasurementTime = _config.Value.OpMeasurementTime;
            Model.MeasurementTime = leafOfAcceptableMeasParams.PeriodsToAverage.ToList();
            Model.SelectedMeasurementTime = Model.MeasurementTime.Count > opMeasurementTime 
                ? Model.MeasurementTime[opMeasurementTime] : Model.MeasurementTime.First();
        }

        private void ReInitializeForSelectedUnit()
        {
            var branchOfAcceptableMeasParams = _treeOfAcceptableMeasParams.Units[Model.SelectedUnit];
            Model.BackscatteredCoefficient = branchOfAcceptableMeasParams.BackscatteredCoefficient;
            Model.RefractiveIndex = branchOfAcceptableMeasParams.RefractiveIndex;

            var currentDistance = Model.SelectedDistance;
            Model.Distances = branchOfAcceptableMeasParams.Distances.Keys.ToList();
            var index = Model.Distances.IndexOf(currentDistance);
            Model.SelectedDistance = index != -1 ? Model.Distances[index] : Model.Distances.First();
        }

        private void ReInitializeForSelectedDistance()
        {
            var leafOfAcceptableMeasParams =
                _treeOfAcceptableMeasParams.Units[Model.SelectedUnit].Distances[Model.SelectedDistance];

            var currentResolution = Model.SelectedResolution;
            Model.Resolutions = leafOfAcceptableMeasParams.Resolutions.ToList();
            var index = Model.Resolutions.IndexOf(currentResolution);
            Model.SelectedResolution = index != -1 ? Model.Resolutions[index] : Model.Resolutions.First();

            var currentPulseDuration = Model.SelectedPulseDuration;
            Model.PulseDurations = leafOfAcceptableMeasParams.PulseDurations.ToList();
            index = Model.PulseDurations.IndexOf(currentPulseDuration);
            Model.SelectedPulseDuration = index != -1 ? Model.PulseDurations[index] : Model.PulseDurations.First();

            var currentPeriodToAverage = Model.SelectedMeasurementTime;
            Model.MeasurementTime = leafOfAcceptableMeasParams.PeriodsToAverage.ToList();
            index = Model.MeasurementTime.IndexOf(currentPeriodToAverage);
            Model.SelectedMeasurementTime = index != -1 ? Model.MeasurementTime[index] : Model.MeasurementTime.First();
        }

        public List<MeasParamByPosition> GetSelectedParameters()
        {
            SaveOtdrParameters();
            var result = new List<MeasParamByPosition>
            {
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.Unit, Position = Model.Units.IndexOf(Model.SelectedUnit)},
                new MeasParamByPosition
                    {Param = ServiceFunctionFirstParam.Bc, Position = (int) (Model.BackscatteredCoefficient * 100)},
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.Ri, Position = (int) (Model.RefractiveIndex * 100000)},
                new MeasParamByPosition
                    {Param = ServiceFunctionFirstParam.Lmax, Position = Model.Distances.IndexOf(Model.SelectedDistance)},
                new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Res, Position = Model.Resolutions.IndexOf(Model.SelectedResolution)
                },
                new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Pulse,
                    Position = Model.PulseDurations.IndexOf(Model.SelectedPulseDuration)
                },
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.IsTime, Position = 1},
                new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Time,
                    Position = Model.MeasurementTime.IndexOf(Model.SelectedMeasurementTime)
                },
            };
            return result;
        }

        public VeexMeasOtdrParameters GetVeexSelectedParameters(bool isGetLineParametersRequest)
        {
            SaveOtdrParameters();
            var result = new VeexMeasOtdrParameters()
            {
                measurementType = @"manual",
                // fastMeasurement = false,
                lasers = new List<Laser>() { new Laser() { laserUnit = Model.SelectedUnit } },
                opticalLineProperties = new OpticalLineProperties()
                {
                    kind = @"point_to_point",
                    lasersProperties = new List<LasersProperty>()
                    {
                        new LasersProperty()
                        {
                            laserUnit = Model.SelectedUnit,
                            backscatterCoefficient = (int)Model.BackscatteredCoefficient,
                            refractiveIndex = Model.RefractiveIndex,
                        }
                    }
                },
                distanceRange = Model.SelectedDistance,
                resolution = Model.SelectedResolution,
                pulseDuration = Model.SelectedPulseDuration,
                averagingTime = Model.SelectedMeasurementTime,
            };

            // for GetLineParametersRequest it should remains NULL
            if (!isGetLineParametersRequest)
                result.highFrequencyResolution = false;
            return result;
        }

        private void SaveOtdrParameters()
        {
            _config.Update(c=>c.OpUnit = Model.Units.IndexOf(Model.SelectedUnit));
            _config.Update(c=>c.OpDistance = Model.Distances.IndexOf(Model.SelectedDistance));
            _config.Update(c=>c.OpResolution = Model.Resolutions.IndexOf(Model.SelectedResolution));
            _config.Update(c=>c.OpPulseDuration = Model.PulseDurations.IndexOf(Model.SelectedPulseDuration));
            _config.Update(c=>c.OpMeasurementTime = Model.MeasurementTime.IndexOf(Model.SelectedMeasurementTime));
        }
    }
}
