using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public interface IOneMeasurementExecutor
    {
        MeasurementModel Model { get; set; }
        bool Initialize(Rtu rtu, bool isForRtu);
        Task Start(TraceLeaf traceLeaf, bool keepOtdrConnection = false);
        void ProcessMeasurementResult(ClientMeasurementResultDto dto);

        event OneIitMeasurementExecutor.MeasurementHandler MeasurementCompleted;
    }
}