using System.Collections.Generic;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class ComponentsReportModel
    {
        public bool IsZoneSelectionEnabled { get; set; }

        public List<Zone> Zones { get; set; } = null!;
        public Zone SelectedZone { get; set; } = null!;

    }
}
