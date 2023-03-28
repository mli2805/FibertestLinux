using System.Collections.Generic;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TraceStateSelectionViewModel : PropertyChangedBase
    {
        public string MinorContext => FiberState.Minor.ToLocalizedString();
        public string MajorContext => FiberState.Major.ToLocalizedString();
        public string CriticalContext => FiberState.Critical.ToLocalizedString();
        public string UserContext => FiberState.User.ToLocalizedString();
        public string FiberBreakContext => FiberState.FiberBreak.ToLocalizedString();
        public string NoFiberContext => FiberState.NoFiber.ToLocalizedString();

        public bool IsMinorChecked { get; set; } = true;
        public bool IsMajorChecked { get; set; } = true;
        public bool IsCriticalChecked { get; set; } = true;
        public bool IsUserChecked { get; set; }
        public bool IsFiberBreakChecked { get; set; } = true;
        public bool IsNoFiberChecked { get; set; } = true;

        public List<FiberState> GetCheckedStates()
        {
            var result = new List<FiberState>();
            if (IsFiberBreakChecked) result.Add(FiberState.FiberBreak);
            if (IsNoFiberChecked) result.Add(FiberState.NoFiber);
            if (IsCriticalChecked) result.Add(FiberState.Critical);
            if (IsMajorChecked) result.Add(FiberState.Major);
            if (IsMinorChecked) result.Add(FiberState.Minor);
            if (IsUserChecked) result.Add(FiberState.User);
            return result;
        }
    }
}
