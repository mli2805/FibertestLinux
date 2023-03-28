using System.Collections.Generic;
using System.Windows;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TceRelationInfo
    {
        public Visibility Visibility { get; set; } = Visibility.Collapsed;

        public List<TceS> Tces { get; set; } = new List<TceS>();
    }
}
