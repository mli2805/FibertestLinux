using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TceTypeSelectionViewModel : PropertyChangedBase
    {
        public List<TceTypeStruct> TceTypes { get; set; } = null!;
        public TceTypeStruct SelectedType { get; set; }

        public void Initialize(List<TceTypeStruct> tceTypes, TceTypeStruct tceTypeStruct)
        {
            TceTypes = tceTypes;
            SelectedType = tceTypes.Contains(tceTypeStruct) ? tceTypeStruct : tceTypes.First();
        }
    }
}
