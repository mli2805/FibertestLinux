using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class GraphVisibilityLevelItem
    {
        public GraphVisibilityLevel Level { get; set; }

        public GraphVisibilityLevelItem(GraphVisibilityLevel level)
        {
            Level = level;
        }

        public override string ToString()
        {
            return Level.GetLocalizedString();
        }
    }
}