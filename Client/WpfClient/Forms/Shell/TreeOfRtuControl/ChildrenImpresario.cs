using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public sealed class ChildrenImpresario : PropertyChangedBase
    {
        private readonly FreePorts _freePorts;

        public ObservableCollection<Leaf> Children { get; }

        public ObservableCollection<Leaf> EffectiveChildren
            => _freePorts.AreVisible ? Children :  new ObservableCollection<Leaf>(Children.Where(c=>!(c is PortLeaf)));

        public ChildrenImpresario(FreePorts freePorts)
        {
            Children = new ObservableCollection<Leaf>();
            Children.CollectionChanged += Children_CollectionChanged;

            _freePorts = freePorts;
            freePorts.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FreePorts.AreVisible))
                    NotifyOfPropertyChange(nameof(EffectiveChildren));
            };
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                NotifyOfPropertyChange(nameof(EffectiveChildren));
        }
    }
}