using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class MonitoringSettingsViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly IWindowManager _windowManager;
        public MonitoringSettingsModel Model { get; set; }

        public int SelectedTabIndex { get; set; }

        private string _messageProp = "";
        public string MessageProp
        {
            get => _messageProp;
            set
            {
                if (value == _messageProp) return;
                _messageProp = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonsEnabled;
        public bool IsButtonsEnabled
        {
            get => _isButtonsEnabled;
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsEditEnabled));
            }
        }

        public bool IsEditEnabled => _currentUser.Role <= Role.Operator && IsButtonsEnabled;

        public MonitoringSettingsViewModel(RtuLeaf rtuLeaf, ILifetimeScope globalScope, 
            CurrentUser currentUser, Model readModel, GrpcC2DService grpcC2DService,
             IWcfServiceCommonC2D commonC2DWcfManager, IWindowManager windowManager,
            MonitoringSettingsModelFactory monitoringSettingsModelFactory)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
            _grpcC2DService = grpcC2DService;
            _commonC2DWcfManager = commonC2DWcfManager;
            _windowManager = windowManager;

            IsButtonsEnabled = true;
            Model = monitoringSettingsModelFactory.Create(rtuLeaf, IsEditEnabled);
            Model.CalculateCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Monitoring_settings;
        }

        public async Task Apply()
        {
            IsButtonsEnabled = false;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                MessageProp = Resources.SID_Command_is_processing___;
                var dto = Model
                    .CreateDto()
                    .AddPortList(Model);
                if (dto.IsMonitoringOn && !dto.Ports.Any())
                {
                    var mb = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_are_no_ports_for_monitoring_);
                    await _windowManager.ShowDialogWithAssignedOwner(mb);
                    IsButtonsEnabled = true;
                    return;
                }
                var resultDto = await _commonC2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
                {
                    var cmd = dto.CreateCommand();
                    var result = await _grpcC2DService.SendEventSourcingCommand(cmd);
                    MessageProp = result.ReturnCode.GetLocalizedString();
                }
                else
                    MessageProp = resultDto.ReturnCode.GetLocalizedWithOsInfo(resultDto.ErrorMessage);
            }
            IsButtonsEnabled = true;
        }

        // Ctrl+B
        // only for traces included into monitoring cycle !
        public async Task<int> ReSendBaseRefsForAllSelectedTraces()  
        {
            MessageProp = Resources.SID_Resending_base_refs_to_RTU___;

            var ports = Model.CreatePortWithTraceList();
            foreach (var port in ports)
            {
                var resendBaseRefDto = new ReSendBaseRefsDto(Model.RtuId, Model.RtuMaker)
                {
                    TraceId = port.TraceId,
                    OtdrId = Model.OtdrId,
                    OtauPortDto = port.OtauPort,
                    BaseRefDtos = new List<BaseRefDto>(),
                };
                foreach (var baseRef in _readModel.BaseRefs.Where(b => b.TraceId == port.TraceId))
                {
                    resendBaseRefDto.BaseRefDtos.Add(new BaseRefDto()
                    {
                        SorFileId = baseRef.SorFileId,

                        Id = baseRef.TraceId,
                        BaseRefType = baseRef.BaseRefType,
                        Duration = baseRef.Duration,
                        SaveTimestamp = baseRef.SaveTimestamp,
                        UserName = baseRef.UserName,
                    });
                }

                var resultDto = await _commonC2DWcfManager.ReSendBaseRefAsync(resendBaseRefDto);
                if (resultDto.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully)
                    MessageProp = Resources.SID_Base_refs_are_sent_to_RTU;
                else
                {
                    MessageProp = Resources.SID_Cannot_send_base_ref_to_RTU;
                    return -1;
                }
            }

            return 0;
        }

        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}
