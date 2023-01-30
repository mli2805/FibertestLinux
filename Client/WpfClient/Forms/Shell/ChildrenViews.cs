using Caliburn.Micro;

namespace Fibertest.WpfClient
{
    public class ChildrenViews : PropertyChangedBase
    {
        private bool _shouldBeClosed;

        public bool ShouldBeClosed
        {
            get => _shouldBeClosed;
            set
            {
                if (value == _shouldBeClosed) return;
                _shouldBeClosed = value;
                NotifyOfPropertyChange();
            }
        }
    }
}