using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class RtuStateModelFactory
    {
        private readonly Model _model;
        private readonly DataCenterConfig _currentDatacenterParameters;

        public RtuStateModelFactory(Model model, DataCenterConfig currentDatacenterParameters)
        {
            _model = model;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        public RtuStateModel Create(RtuLeaf rtuLeaf)
        {
            var rtu = _model.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return null;

            var rtuStateModel = new RtuStateModel();
            rtuStateModel.Title = rtuLeaf.Title;
            rtuStateModel.ServerTitle = _currentDatacenterParameters.General.ServerTitle;

            rtuStateModel.MainAddress = rtu.MainChannel.ToStringA();
            rtuStateModel.MainAddressState = rtuLeaf.MainChannelState;

            rtuStateModel.HasReserveAddress = rtu.IsReserveChannelSet;
            rtuStateModel.ReserveAddress = rtu.IsReserveChannelSet ? rtu.ReserveChannel.ToStringA() : Resources.SID_None;
            rtuStateModel.ReserveAddressState = rtuLeaf.ReserveChannelState;

            rtuStateModel.FullPortCount = rtuLeaf.FullPortCount;
            rtuStateModel.OwnPortCount = rtuLeaf.OwnPortCount;

            rtuStateModel.MonitoringMode = rtuLeaf.MonitoringState.ToLocalizedString();

            var traceCount = 0;
            rtuStateModel.Ports = PreparePortLines(rtuLeaf.ChildrenImpresario.Children, "",  ref traceCount);
            rtuStateModel.SetWorstTraceStateAsAggregate();
            rtuStateModel.TraceCount = traceCount;
            rtuStateModel.BopCount = rtuLeaf.ChildrenImpresario.Children.Count(l=>l is OtauLeaf);
            rtuStateModel.BopState = rtuLeaf.BopState;

            rtuStateModel.CurrentMeasurementStep = rtuLeaf.MonitoringState == MonitoringState.Off 
                ? Resources.SID_No_measurement 
                : Resources.SID_Waiting_for_data;
            return rtuStateModel;
        }

        private List<PortLineModel> PreparePortLines(ObservableCollection<Leaf> leaves, string mainPort,  ref int traceCount)
        {
            var result = new List<PortLineModel>();
            foreach (var leaf in leaves)
            {
                if (leaf is PortLeaf portLeaf)
                    result.Add(PreparePortLine(portLeaf, mainPort));

                if (leaf is TraceLeaf traceLeaf && traceLeaf.PortNumber > 0)
                {
                    result.Add(PreparePortLine(traceLeaf, mainPort));
                    traceCount++;
                }

                if (leaf is OtauLeaf bopLeaf)
                    result.AddRange(
                        PreparePortLines(bopLeaf.ChildrenImpresario.Children,
                            mainPort + bopLeaf.MasterPort + @"-", ref traceCount));
            }
            return result;
        }

        private PortLineModel PreparePortLine(PortLeaf portLeaf, string mainPort)
        {
            return new PortLineModel()
            {
                Number = mainPort + portLeaf.PortNumber,
                TraceTitle = Resources.SID_None,
                TraceState = FiberState.Unknown,
            };
        }

        private PortLineModel PreparePortLine(TraceLeaf traceLeaf, string mainPort)
        {
            var lastMeasurement = _model.Measurements.LastOrDefault(m=>m.TraceId == traceLeaf.Id);
            return new PortLineModel()
            {
                Number = mainPort + traceLeaf.PortNumber,
                TraceId = traceLeaf.Id,
                TraceTitle = traceLeaf.Title,
                TraceState = traceLeaf.TraceState,
                Timestamp = lastMeasurement?.EventRegistrationTimestamp,
                LastSorFileId = lastMeasurement?.SorFileId.ToString(),
            };
        }
    }
}