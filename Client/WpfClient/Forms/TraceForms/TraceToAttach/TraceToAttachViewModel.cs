using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class TraceToAttachViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DCommonWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;
        private OtauPortDto _otauPortDto;
        private Rtu _rtu;

        private string _searchMask;
        public string SearchMask    
        {
            get => _searchMask;
            set
            {
                if (value == _searchMask) return;
                _searchMask = value;
                NotifyOfPropertyChange();

                Choices.Clear();
                _readModel.Traces
                    .Where(t => t.RtuId == _rtu.Id && t.Port < 1 && t.ZoneIds.Contains(_currentUser.ZoneId)
                                    && t.Title.Contains(_searchMask))
                    .ToList()
                    .ForEach(t => Choices.Add(t));
            }
        }

        public ObservableCollection<Trace> Choices { get; set; } = new ObservableCollection<Trace>();

        private Trace _selectedTrace;
        public Trace SelectedTrace
        {
            get => _selectedTrace;
            set
            {
                if (Equals(value, _selectedTrace)) return;
                _selectedTrace = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonsEnabled = true;

        public bool IsButtonsEnabled
        {
            get => _isButtonsEnabled;
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceToAttachViewModel(ILifetimeScope globalScope, Model readModel, CurrentUser currentUser,
            IWcfServiceCommonC2D c2DCommonWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DCommonWcfManager = c2DCommonWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public void Initialize(Rtu rtu, OtauPortDto otauPortDto)
        {
            _rtu = rtu;
            _otauPortDto = otauPortDto;
            Choices.Clear();
            _readModel.Traces
                .Where(t => t.RtuId == rtu.Id && t.Port < 1 && t.ZoneIds.Contains(_currentUser.ZoneId))
                .ToList()
                .ForEach(t => Choices.Add(t));
            SelectedTrace = Choices.FirstOrDefault();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Not_attached_traces_list;
        }

        public async Task FullAttach()
        {
            if (SelectedTrace == null) return;

            IsButtonsEnabled = false;
            var dto = new AttachTraceDto()
            {
                ClientConnectionId = _currentUser.ConnectionId,
                RtuMaker = _rtu.RtuMaker,
                TraceId = SelectedTrace.TraceId,
                OtauPortDto = _otauPortDto,
                MainOtauPortDto = new OtauPortDto( _otauPortDto.MainCharonPort, true)
                {
                    OtauId = _rtu.MainVeexOtau.id,
                },
            };

            RequestAnswer result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _c2DCommonWcfManager.AttachTraceAndSendBaseRefs(dto);
            }

            if (result.ReturnCode != ReturnCode.Ok)
            {
                var errs = new List<string>
                {
                    result.ReturnCode == ReturnCode.D2RGrpcOperationError
                        ? Resources.SID_Cannot_send_base_refs_to_RTU
                        : Resources.SID_Base_reference_assignment_failed
                };

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    errs.Add(result.ErrorMessage);

                var vm = new MyMessageBoxViewModel(MessageType.Error, errs);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }

            IsButtonsEnabled = true;
            await TryCloseAsync();
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }
    }
}