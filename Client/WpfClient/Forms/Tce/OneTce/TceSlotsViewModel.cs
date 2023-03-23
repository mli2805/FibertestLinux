using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TceSlotsViewModel : PropertyChangedBase
    {
        public List<SlotViewModel> Slots { get; set; } = null!;

        private SlotViewModel _selectedSlot = null!;
        public SlotViewModel SelectedSlot    
        {
            get => _selectedSlot;
            set
            {
                if (Equals(value, _selectedSlot)) return;
                _selectedSlot = value;
                NotifyOfPropertyChange();
            }
        }

        public void Initialize(Model readModel, TceS tce, Func<Trace, bool> isTraceLinked, bool isEnabled)
        {
            Slots = new List<SlotViewModel>();
            foreach (var slotPosition in tce.TceTypeStruct.SlotPositions)
            {
                var slot = new SlotViewModel(readModel);
                slot.Initialize(tce, slotPosition, isTraceLinked, isEnabled);
                Slots.Add(slot);

            }

            SelectedSlot = Slots.First();
        }

    }
}
