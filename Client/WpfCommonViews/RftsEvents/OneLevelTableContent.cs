using System.Collections.Generic;

namespace Fibertest.WpfCommonViews
{
    public class OneLevelTableContent
    {
        public Dictionary<int, string?[]> Table { get; set; } = new Dictionary<int, string?[]>();
        public bool IsFailed { get; set; }
        public string? FirstProblemLocation { get; set; }
    }
}