using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fibertest.WpfClient
{
    public class GrpcInClientProcessor
    {
        private readonly ILogger _logger;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly AutoBaseViewModel _autoBaseViewModel;
        private readonly RtuAutoBaseViewModel _rtuAutoBaseViewModel;

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };

        public GrpcInClientProcessor(ILogger logger, ClientMeasurementViewModel clientMeasurementViewModel,
            AutoBaseViewModel autoBaseViewModel, RtuAutoBaseViewModel rtuAutoBaseViewModel)
        {
            _logger = logger;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _autoBaseViewModel = autoBaseViewModel;
            _rtuAutoBaseViewModel = rtuAutoBaseViewModel;
        }

        public void Apply(string json)
        {
            var o = JsonConvert.DeserializeObject(json, JsonSerializerSettings);
            switch (o)
            {
                case ClientMeasurementResultDto dto:
                    ProcessMeasurementResult(dto);
                    break;
                case CurrentMonitoringStepDto dto:
                    ProcessCurrentMonitoringStep(dto);
                    break;
                default: return;
            }
        }

        private void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            _logger.Info(Logs.Client, $"Client measurement result {dto.ReturnCode.GetLocalizedString()}");
            if (_clientMeasurementViewModel.IsOpen)
                Task.Factory.StartNew(() => _clientMeasurementViewModel.ShowResult(dto));
            if (_autoBaseViewModel.IsOpen)
                Task.Factory.StartNew(() => _autoBaseViewModel.OneMeasurementExecutor.ProcessMeasurementResult(dto));
            if (_rtuAutoBaseViewModel.IsOpen)
                Task.Factory.StartNew(() => _rtuAutoBaseViewModel.WholeRtuMeasurementsExecutor.ProcessMeasurementResult(dto));
        }

        private void ProcessCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _logger.Info(Logs.Client, $"Current monitoring step {dto.Step}");

        }
    }
}
