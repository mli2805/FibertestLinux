using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class MonitoringSettingsModelFactory
    {
        private RtuLeaf _rtuLeaf = null!;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        public MonitoringSettingsModelFactory(Model readModel, CurrentUser currentUser)
        {
            _readModel = readModel;
            _currentUser = currentUser;
        }

        public MonitoringSettingsModel Create(RtuLeaf rtuLeaf, bool isEditEnabled)
        {
            _rtuLeaf = rtuLeaf;
            var rtu = _readModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            var model = new MonitoringSettingsModel()
            {
                RtuId = _rtuLeaf.Id,
                RtuTitle = _rtuLeaf.Title,
                RtuMaker = _rtuLeaf.RtuMaker,
                OtdrId = rtu.OtdrId,
                MainVeexOtau = rtu.MainVeexOtau,
                RealOtdrAddress = GetRealOtdrAddress(),
                IsMonitoringOn = _rtuLeaf.MonitoringState == MonitoringState.On,
                Charons = PrepareMonitoringCharonModels(isEditEnabled),
            };
            model.Frequencies.InitializeComboboxes(rtu.FastSave, rtu.PreciseMeas, rtu.PreciseSave);
            return model;
        }

        private string? GetRealOtdrAddress()
        {
            return _readModel.Rtus.FirstOrDefault(r => r.Id == _rtuLeaf.Id)?.OtdrNetAddress.Ip4Address;
        }

        private List<MonitoringCharonModel> PrepareMonitoringCharonModels(bool isEditEnabled)
        {
            var charons = new List<MonitoringCharonModel> {PrepareMainCharonModel(isEditEnabled)};
            foreach (var leaf in _rtuLeaf.ChildrenImpresario.Children)
            {
                var otauLeaf = leaf as OtauLeaf;
                if (otauLeaf != null)
                {
                    charons.Add(PrepareBopCharonModel(otauLeaf, isEditEnabled));
                }
            }
            return charons;
        }

        private MonitoringCharonModel PrepareMainCharonModel(bool isEditEnabled)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            var mainCharonModel = new MonitoringCharonModel(_rtuLeaf.Serial!)
            {
                Title = _rtuLeaf.Title,
                IsMainCharon = true,
                OtauId = rtu.MainVeexOtau.id,
                Ports = PrepareMonitoringPortModels(_rtuLeaf),
                IsEditEnabled = isEditEnabled,
            };
            return mainCharonModel;
        }

        private MonitoringCharonModel PrepareBopCharonModel(OtauLeaf otauLeaf, bool isEditEnabled)
        {
            var otau = _readModel.Otaus.First(o => o.Id == otauLeaf.Id);
            var bopCharonModel = new MonitoringCharonModel(otauLeaf.Serial!)
            {
                Title = otauLeaf.OtauNetAddress!.ToStringA(),
                IsMainCharon = false,
                OtauId = otau.Id.ToString(),
                MainCharonPort = otau.MasterPort,
                Ports = PrepareMonitoringPortModels(otauLeaf),
                IsEditEnabled = isEditEnabled,
            };
            return bopCharonModel;
        }

        private List<MonitoringPortModel> PrepareMonitoringPortModels(IPortOwner portOwner)
        {
            var result = new List<MonitoringPortModel>();
            for (int i = 0; i < portOwner.OwnPortCount; i++)
            {
                if (portOwner.ChildrenImpresario.Children[i] is TraceLeaf traceLeaf)
                {
                    var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
                    result.Add(new MonitoringPortModel()
                    {
                        PortNumber = i+1,
                        TraceId = traceLeaf.Id,
                        TraceTitle = traceLeaf.Title,
                        PreciseBaseSpan = trace.PreciseDuration,
                        FastBaseSpan = trace.FastDuration,
                        // AdditionalBaseSpan = trace.AdditionalDuration,
                        IsIncluded = trace.IsIncludedInMonitoringCycle,
                        IsInCurrentUserZone = trace.ZoneIds.Contains(_currentUser.ZoneId),
                    });
                }
                else
                {
                    result.Add(new MonitoringPortModel()
                    {
                        PortNumber = i+1,
                        TraceTitle = "",
                    });

                }
            }
            return result;
        }
    }
}
