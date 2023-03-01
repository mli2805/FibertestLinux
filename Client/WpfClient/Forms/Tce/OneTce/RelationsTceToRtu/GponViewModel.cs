using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class GponViewModel : PropertyChangedBase
    {
        private readonly Model _readModel;
        public GponModel GponInWork { get; set; } = null!;


        public List<Rtu> Rtus { get; set; } = null!;
        public ObservableCollection<Otau> Otaus { get; set; } = new();

        private Func<Trace, bool> _isTraceLinked = null!;

        public GponViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(GponModel gponModel, Func<Trace, bool> isTraceLinked)
        {
            GponInWork = gponModel;
            GponInWork.PropertyChanged += GponInWork_PropertyChanged;
            _isTraceLinked = isTraceLinked;
        }

        private void GponInWork_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Rtu" && GponInWork.Rtu != null)
            {
                var rtu = Rtus.First(r => r.Id == GponInWork.Rtu.Id);
                CollectOtausOnRtuChanged(rtu);
                GponInWork.Otau = Otaus.First();
                GponInWork.OtauPortNumberStr = "";
                GponInWork.Trace = null;
            }

            else if (e.PropertyName == @"Otau" && GponInWork.Rtu != null && GponInWork.Otau != null)
            {
                GponInWork.OtauPortNumberStr = "";
                GponInWork.Trace = null;
            }

            else if (e.PropertyName == @"OtauPortNumberStr" && GponInWork.Rtu != null
                                                            && GponInWork.Otau != null)
            {
                if (GponInWork.OtauPortNumberStr != "")
                {
                    var trace = FindTrace();
                    if (trace == null)
                        GponInWork.Trace = null;

                    else
                    {
                        GponInWork.TraceAlreadyLinked = _isTraceLinked(trace)
                            ? string.Format(
                                Resources.SID_Trace___0___has_already_linked_to_another_interface, trace.Title)
                            : "";
                        GponInWork.Trace = trace;
                    }
                }
                else
                {
                    GponInWork.Trace = null;
                }
            }
        }

        private Trace? FindTrace()
        {
            if (GponInWork.Rtu!.RtuMaker == RtuMaker.VeEX && GponInWork.Otau!.Id == Guid.Empty // VEEX main otau
                || GponInWork.Rtu.RtuMaker == RtuMaker.IIT && GponInWork.Otau!.Id == GponInWork.Otau.RtuId) // IIT main otau
            {
                return _readModel.Traces.FirstOrDefault(t => t.RtuId == GponInWork.Rtu.Id
                                                              && t.OtauPort != null
                                                              && t.OtauPort.IsPortOnMainCharon
                                                              && t.OtauPort.OpticalPort == int.Parse(GponInWork.OtauPortNumberStr!));
            }

            // bop
            return _readModel.Traces.FirstOrDefault(t => t.RtuId == GponInWork.Rtu.Id
                                                          && t.OtauPort != null
                                                          && t.OtauPort.OtauId == GponInWork.Otau!.Id.ToString()
                                                          && t.OtauPort.OpticalPort == int.Parse(GponInWork.OtauPortNumberStr!));
        }

        public void CollectOtausOnRtuChanged(Rtu? rtu)
        {
            if (rtu == null) return;
            Otaus.Clear();
            if (rtu.RtuMaker == RtuMaker.IIT)
            {
                var mainOtau = new Otau() { Id = rtu.Id, RtuId = rtu.Id, PortCount = rtu.OwnPortCount, IsMainOtau = true };
                Otaus.Add(mainOtau);
            }
            foreach (var otau in _readModel.Otaus.Where(o => o.RtuId == rtu.Id))
            {
                Otaus.Add(otau);
            }
        }

        public void ClearRelation()
        {
            GponInWork.ClearRelation();
        }

        public GponPortRelation GetGponPortRelation()
        {
            var opticalPort = int.Parse(GponInWork.OtauPortNumberStr!);
            OtauPortDto otauPortDto;

            if (GponInWork.Rtu!.RtuMaker == RtuMaker.VeEX && GponInWork.Otau!.Id == Guid.Empty) // VEEX main otau
            {
                otauPortDto = new OtauPortDto(opticalPort, true);
                otauPortDto.OtauId = GponInWork.Otau.VeexRtuMainOtauId;
            }
            else if (GponInWork.Rtu.RtuMaker == RtuMaker.IIT && GponInWork.Otau!.Id == GponInWork.Otau.RtuId) // IIT main otau
            {
                otauPortDto = new OtauPortDto(opticalPort, true);
                otauPortDto.OtauId = GponInWork.Otau.RtuId.ToString();
                otauPortDto.Serial = GponInWork.Rtu.Serial;
            }
            else
            {
                otauPortDto = new OtauPortDto(opticalPort, false);
                otauPortDto.OtauId = GponInWork.Otau!.Id.ToString();
                var otau = _readModel.Otaus.FirstOrDefault(o => o.Id == GponInWork.Otau.Id);
                if (GponInWork.Rtu.RtuMaker == RtuMaker.IIT)
                    otauPortDto.Serial = otau?.Serial;
                if (otau != null)
                    otauPortDto.NetAddress = otau.NetAddress.Clone();
            }

            return new GponPortRelation()
            {
                TceId = GponInWork.Tce.Id,
                SlotPosition = GponInWork.SlotPosition,
                GponInterface = GponInWork.GponInterface,
                RtuId = GponInWork.Rtu.Id,
                RtuMaker = GponInWork.Rtu.RtuMaker,
                OtauPortDto = otauPortDto,
                TraceId = GponInWork.Trace!.TraceId,
            };
        }

    }
}
