using System.Collections.Generic;

namespace Fibertest.WpfCommonViews
{
    public class LevelsContent
    {
        public bool IsMinorExists { get; set; }
        public bool IsMajorExists { get; set; }
        public bool IsCriticalExists { get; set; }
        public bool IsUsersExists { get; set; }

        public RftsEventsOneLevelViewModel? MinorLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel? MajorLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel? CriticalLevelViewModel { get; set; }
        public RftsEventsOneLevelViewModel? UsersLevelViewModel { get; set; }

        public RftsEventsOneLevelViewModel? GetByIndex(int index)
        {
            switch (index)
            {
                case 0: return MinorLevelViewModel;
                case 1: return MajorLevelViewModel;
                case 2: return CriticalLevelViewModel;
                case 3: return UsersLevelViewModel;
            }

            return null;
        }

        public IEnumerable<RftsEventsOneLevelViewModel> GetAll()
        {
            if (IsMinorExists) yield return MinorLevelViewModel!;
            if (IsMajorExists) yield return MajorLevelViewModel!;
            if (IsCriticalExists) yield return CriticalLevelViewModel!;
            if (IsUsersExists) yield return UsersLevelViewModel!;
        }

    }
}