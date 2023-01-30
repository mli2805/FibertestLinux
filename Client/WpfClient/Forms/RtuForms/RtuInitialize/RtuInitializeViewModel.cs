﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    public class RtuInitializeViewModel : Screen
    {
        public RtuInitializeModel FullModel { get; set; }

        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _wcfServiceCommonC2D;
        private readonly IWcfServiceDesktopC2D _wcfServiceDesktopC2D;
        private readonly ILogger<RtuInitializeModel> _logger; 
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        private bool _isIdle;
        public bool IsIdle
        {
            get => _isIdle;
            set
            {
                if (value == _isIdle) return;
                _isIdle = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsInitializationPermitted));
            }
        }

        private bool _isCloseEnabled;
        public bool IsCloseEnabled
        {
            get { return _isCloseEnabled; }
            set
            {
                if (value == _isCloseEnabled) return;
                _isCloseEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsInitializationPermitted => _currentUser.Role <= Role.Operator && IsIdle;

        public RtuInitializeViewModel(ILifetimeScope globalScope, CurrentUser currentUser, Model readModel,
            IWindowManager windowManager, IWcfServiceCommonC2D wcfServiceCommonC2D, IWcfServiceDesktopC2D wcfServiceDesktopC2D,
            ILogger<RtuInitializeModel> logger, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
            IsIdle = true;
            IsCloseEnabled = true;
            _windowManager = windowManager;
            _wcfServiceCommonC2D = wcfServiceCommonC2D;
            _wcfServiceDesktopC2D = wcfServiceDesktopC2D;
            _logger = logger;
            _commonStatusBarViewModel = commonStatusBarViewModel;

            FullModel = _globalScope.Resolve<RtuInitializeModel>();
            FullModel.StartFromRtu(rtuLeaf.Id);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public async Task InitializeAndSynchronize()
        {
            await Do(true);
        }

        public async Task InitializeRtu()
        {
            await Do(false);
        }

        private async Task Do(bool isSynchronizationRequired)
        {
            if (!FullModel.Validate()) return;

            try
            {
                IsIdle = false;
                IsCloseEnabled = false;
                RtuInitializedDto result;

                using (_globalScope.Resolve<IWaitCursor>())
                {
                    if (!await FullModel.CheckConnectionBeforeInitialization())
                        return;
                    var rtuMaker = FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == (int)TcpPorts.RtuListenTo
                        ? RtuMaker.IIT
                        : RtuMaker.VeEX;
                    _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_RTU_is_being_initialized___;

                    var initializeRtuDto = FullModel.CreateDto(rtuMaker, _currentUser);
                    initializeRtuDto.IsSynchronizationRequired = isSynchronizationRequired;
                    result = await _wcfServiceCommonC2D.InitializeRtuAsync(initializeRtuDto);
                }
                _commonStatusBarViewModel.StatusBarMessage2 = "";

                if (result.ReturnCode == ReturnCode.RtuUnauthorizedAccess)
                {
                    var vm = new RtuAskSerialViewModel();
                    vm.Initialize(!FullModel.OriginalRtu.IsInitialized,
                        FullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().ToStringA(), result.Serial);
                    await _windowManager.ShowDialogWithAssignedOwner(vm);
                    if (!vm.IsSavePressed) return;
                    FullModel.OriginalRtu.Serial = vm.Serial.ToUpper();
                    await InitializeRtu();
                }
                else
                {
                    _logger.LogInfo(Logs.Client,result.CreateLogMessage());

                    if (result.IsInitialized)
                        FullModel.UpdateWithDto(result);

                    if (result.IsInitialized && isSynchronizationRequired)
                        await SynchronizeBaseRefs();

                    _windowManager.ShowDialogWithAssignedOwner(result.CreateMessageBox(FullModel.OriginalRtu.Title));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(Logs.Client,$@"InitializeRtu : {e.Message}");
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_RTU_initialization_error_);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            finally
            {
                IsIdle = true;
                IsCloseEnabled = true;
                _commonStatusBarViewModel.StatusBarMessage2 = "";
            }
        }

        private async Task SynchronizeBaseRefs()
        {
            var commands = new List<object>();
            foreach (var veexTest in _readModel.VeexTests)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == veexTest.TraceId);
                if (trace != null && trace.RtuId == FullModel.OriginalRtu.Id)
                    commands.Add(new RemoveVeexTest() { TestId = veexTest.TestId });
            }
            await _wcfServiceDesktopC2D.SendCommandsAsObjs(commands);


            using (_globalScope.Resolve<IWaitCursor>())
            {
                var list = _readModel.CreateReSendDtos(FullModel.OriginalRtu, _currentUser).ToList();
                foreach (var reSendBaseRefsDto in list)
                {
                    _commonStatusBarViewModel.StatusBarMessage2 
                        = string.Format(Resources.SID_Sending_base_refs_for_port__0_, reSendBaseRefsDto.OtauPortDto.ToStringB());
                    var resultDto = await _wcfServiceCommonC2D.ReSendBaseRefAsync(reSendBaseRefsDto);
                    _commonStatusBarViewModel.StatusBarMessage2 
                        = $@"Sending base refs for port {reSendBaseRefsDto.OtauPortDto.ToStringB()} {resultDto.ReturnCode}";
                }
            }
        }

        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}