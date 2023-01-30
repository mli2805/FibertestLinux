using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class SlotViewModel : Screen
    {
        private readonly Model _readModel;
        private TceS _tce;
        public int SlotPosition { get; set; }

        private int _interfaceCount;
        public int InterfaceCount
        {
            get => _interfaceCount;
            set
            {
                if (value == _interfaceCount) return;
                _interfaceCount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Title));
            }
        }

        private int _relationsOnSlot;
        public int RelationsOnSlot
        {
            get => _relationsOnSlot;
            set
            {
                if (value == _relationsOnSlot) return;
                _relationsOnSlot = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Title));
            }
        }

        public string Title => InterfaceCount == 0 ? $@"{SlotPosition} -  " : $@"{SlotPosition} - {InterfaceCount} / {RelationsOnSlot}";

        public List<Rtu> Rtus { get; set; }

        public ObservableCollection<GponViewModel> Gpons { get; set; } =
            new ObservableCollection<GponViewModel>();

        private Func<Trace, bool> _isTraceLinked;

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

        public SlotViewModel(Model readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(TceS tce, int slotPosition, Func<Trace, bool> isTraceLinked, bool isEnabled)
        {
            Rtus = _readModel.Rtus;
            _tce = new TceS(tce);
            SlotPosition = slotPosition;
            InterfaceCount = tce.Slots.First(s => s.Position == slotPosition).GponInterfaceCount;

            _isTraceLinked = isTraceLinked;
            IsEnabled = isEnabled;

            InitializeGpons();
        }

        private void InitializeGpons()
        {
            for (int i = _tce.TceTypeStruct.GponInterfaceNumerationFrom; i < InterfaceCount + _tce.TceTypeStruct.GponInterfaceNumerationFrom; i++)
            {
                var lineViewModel = new GponViewModel(_readModel) { Rtus = _readModel.Rtus };
                var lineModel = new GponModel()
                {
                    GponInterface = i,
                    SlotPosition = SlotPosition,
                    Tce = _tce
                };

                var relation = _readModel.GponPortRelations.FirstOrDefault(r => r.TceId == _tce.Id
                    && r.SlotPosition == SlotPosition
                    && r.GponInterface == i);

                if (relation != null)
                {
                    RelationsOnSlot++;

                    lineModel.Rtu = _readModel.Rtus.First(r => r.Id == relation.RtuId);

                    lineViewModel.CollectOtausOnRtuChanged(lineModel.Rtu);

                    lineModel.Otau = lineViewModel.Otaus.FirstOrDefault(o => o.Id.ToString() == relation.OtauPortDto.OtauId) ??
                                          // Veex RTU main otau has no Id (Veex id are the same for all main otaus)
                                          lineViewModel.Otaus.FirstOrDefault(o => o.RtuId == lineModel.Rtu.Id && o.IsMainOtau);

                    lineModel.OtauPortNumberStr = relation.OtauPortDto.OpticalPort.ToString();
                    lineModel.Trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == relation.TraceId);
                }

                lineViewModel.Initialize(lineModel, _isTraceLinked);
                Gpons.Add(lineViewModel);

                lineViewModel.GponInWork.PropertyChanged += GponInWork_PropertyChanged;
            }
        }

        private void GponInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Trace")
            {
                // RelationsOnSlot = Gpons.Count(g => !string.IsNullOrEmpty(g.GponInWork.OtauPortNumberStr));
                RelationsOnSlot = Gpons.Count(g => g.GponInWork.Trace != null);
            }
        }

        public void ChangeInterfaceCount()
        {
            var oldCount = _tce.Slots.First(s => s.Position == SlotPosition).GponInterfaceCount;
            _tce.Slots.First(s => s.Position == SlotPosition).GponInterfaceCount = InterfaceCount;

            if (oldCount < InterfaceCount)
                for (int i = oldCount + _tce.TceTypeStruct.GponInterfaceNumerationFrom; i < InterfaceCount + _tce.TceTypeStruct.GponInterfaceNumerationFrom; i++)
                {
                    CreateNewGponInterface(i);
                }
            else
            {
                var forRemoval = Gpons
                    .Where(g => g.GponInWork.GponInterface - _tce.TceTypeStruct.GponInterfaceNumerationFrom >= InterfaceCount).ToList();
                foreach (var gponViewModel in forRemoval)
                {
                    if (gponViewModel.GponInWork.Trace != null) RelationsOnSlot--;
                    Gpons.Remove(gponViewModel);
                }
            }
        }

        private void CreateNewGponInterface(int i)
        {
            var line = new GponViewModel(_readModel) { Rtus = _readModel.Rtus };
            var lineModel = new GponModel()
            {
                GponInterface = i,
                SlotPosition = SlotPosition,
                Tce = _tce
            };

            line.Initialize(lineModel, _isTraceLinked);
            Gpons.Add(line);
            line.GponInWork.PropertyChanged += GponInWork_PropertyChanged;
        }

        public TceSlot GetTceSlot()
        {
            return new TceSlot() { Position = SlotPosition, GponInterfaceCount = InterfaceCount };
        }

        public IEnumerable<GponPortRelation> GetGponPortsRelations()
        {
            return Gpons.Where(g => g.GponInWork.Trace != null && 
                                                string.IsNullOrEmpty(g.GponInWork.TraceAlreadyLinked))
                        .Select(gponViewModel => gponViewModel.GetGponPortRelation());
        }
    }
}
