using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Fibertest.Utils.Setup;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;

namespace Fibertest.WpfClient
{
    // Could be done for TraceLeaf and PortLeaf
    public class CommonActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly ILogger _logger; 
        private readonly CurrentUser _currentUser;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;

        public CommonActions(ILifetimeScope globalScope, IWritableConfig<ClientConfig> config, 
            ILogger logger, CurrentUser currentUser,
            Model readModel, IWindowManager windowManager, IWcfServiceCommonC2D c2RWcfManager,
            ClientMeasurementViewModel clientMeasurementViewModel)
        {
            _globalScope = globalScope;
            _config = config;
            _logger = logger;
            _currentUser = currentUser;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2RWcfManager = c2RWcfManager;
        }

        public async Task MeasurementClientAction(object param)
        {
            var parent = GetParent(param);
            if (parent == null) return;

            // RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            // if (!await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(rtuLeaf.Id, rtuLeaf.Title, RtuOccupation.DoMeasurementClient))
            //     return;

            if (await _clientMeasurementViewModel.Initialize(parent, GetPortNumber(param)))
               await _windowManager.ShowDialogWithAssignedOwner(_clientMeasurementViewModel);

            // await _globalScope.Resolve<IRtuHolder>()
            //     .SetRtuOccupationState(rtuLeaf.Id, rtuLeaf.Title, RtuOccupation.None);
        }

        public async Task MeasurementRftsReflectAction(object param)
        {
            var parent = GetParent(param);
            if (parent == null) return;

            var prepareResult = await PrepareRtuForMeasurementReflect(parent, GetPortNumber(param));
            if (prepareResult == null) return;

            var rootPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            System.Diagnostics.Process.Start(rootPath + @"\RftsReflect\Reflect.exe",
                $@"-fnw -n {prepareResult.Ip4Address} -p {prepareResult.Port}");
        }

        private Leaf? GetParent(object param)
        {
            if (param is Leaf leaf)
                return leaf.Parent;
            return null;
        }

        private int GetPortNumber(object param)
        {
            if (param is IPortNumber leaf)
                return leaf.PortNumber;
            return 0;
        }


        /*
         * Old fashioned RTU has main address 192.168.96.58 which is different from its otau address 192.168.96.59
         *      mainCharon = 192.168.96.59 : 23    addressOfCharonWithThisPort = 192.168.96.59 : 23
         *          and than otdr address = 192.168.96.59 : 1500
         *
         * New RTU (MAK100) has main address 172.16.5.53 and its otau could be addressed by the same address,
         * but in otau address stored value 192.168.88.101
         *      mainCharon = 172.16.5.53 : 23    addressOfCharonWithThisPort = 172.16.5.53 : 23
         *          and than otdr address = 172.16.5.53 : 1500
         *
         * Every of these RTU could be augmented with BOP (let it be that additional otau has address 172.16.5.57 : 11842)
         * but OTDR is always part of RTU, not of BOP
         *
         *      so Old RTU with BOP:  mainCharon = 192.168.96.59 : 23      addressOfCharonWithThisPort = 172.16.5.57 : 11842
         *          and than otdr address = 192.168.96.59 : 1500
         *
         *      while MAK100 with BOP:   mainCharon = 172.16.5.53 : 23      addressOfCharonWithThisPort = 172.16.5.57 : 11842
         *          and than otdr address = 172.16.5.53 : 1500
         *
          * UCC (БУС) has main address 172.16.4.8
         *  and Fibertest Client addresses it 172.16.4.8 : 11842
         *  its OTAU could be addressed
         *  - from UCC as 192.168.88.101 : 23
         *  - from outside as 172.16.4.8 : 23
         *  but its OTDR is a separate device (АТР), which could be addressed
         *  - from UCC as 192.168.88.102 : 10001
         *  - from outside as 172.16.4.8 : 10001
         *
         *
         *  Veex RTU4000 has main address someIp : 80
         *  it could be augmented with additional OTAU (address in not important)
         *  first we send command to enable proxy mode (e.i. RTU stop measurements and ready for attachment from the outside)
         *  send we send command to the RTU to toggle main and additional otaus
         *
         */
        public async Task<NetAddress?> PrepareRtuForMeasurementReflect(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return null;

            // Measurement Reflect must check if RTU is busy
            // but should not block RTU, it has lowest priority, other operations can interrupt it
            if (!await _globalScope.Resolve<IRtuHolder>().SetRtuOccupationState(rtu.Id, rtu.Title, RtuOccupation.None))
                return null;

            return rtu.RtuMaker == RtuMaker.IIT
                ? await PrepareIitRtu(rtuLeaf, rtu, parent, portNumber)
                : await PrepareVeexRtu(rtu, parent, portNumber);
        }

        private async Task<NetAddress?> PrepareIitRtu(RtuLeaf rtuLeaf, Rtu rtu, Leaf parent, int portNumber)
        {
            var isMak100 = rtuLeaf.OtauNetAddress.Ip4Address == @"192.168.88.101";
            var isUcc = rtuLeaf.OtauNetAddress.Ip4Address == @"192.168.88.102"; // БУС

            var mainCharonAddress = isMak100 || isUcc
                ? rtu.MainChannel.Clone()
                : rtu.OtdrNetAddress.Clone();
            mainCharonAddress.Port = 23;
            var mainCharon = new Charon(mainCharonAddress, true, _config.Value.CharonConfig, _logger)
            {
                OwnPortCount = rtuLeaf.OwnPortCount,
                Serial = rtuLeaf.Serial,
            };

            string serialOfCharonWithThisPort;
            if (parent is OtauLeaf otauLeaf)
            {
                serialOfCharonWithThisPort = otauLeaf.Serial;

                var bopCharon = new Charon(otauLeaf.OtauNetAddress, false, _config.Value.CharonConfig,
                     _logger);
                bopCharon.Serial = otauLeaf.Serial;
                bopCharon.OwnPortCount = otauLeaf.OwnPortCount;
                mainCharon.Children = new Dictionary<int, Charon> { { otauLeaf.MasterPort, bopCharon } };
            }
            else
            {
                serialOfCharonWithThisPort = mainCharon.Serial;
            }

            var toggleResult = await ToggleToPort(mainCharon, serialOfCharonWithThisPort, portNumber);
            return toggleResult ? new NetAddress(mainCharonAddress.Ip4Address, isUcc ? 10001 : 1500) : null;
        }

        private async Task<NetAddress?> PrepareVeexRtu(Rtu rtu, Leaf parent, int portNumber)
        {
            var dto = new PrepareReflectMeasurementDto(rtu.Id, rtu.RtuMaker)
            {
                OtdrId = rtu.OtdrId,
            };

            if (parent is OtauLeaf otauLeaf)
            {
                dto.OtauPortDto = new OtauPortDto(portNumber, false)
                {
                    MainCharonPort = otauLeaf.MasterPort,
                    OtauId = _readModel.Otaus.First(o => o.Id == otauLeaf.Id).Id.ToString(),
                    Serial = otauLeaf.Serial,
                };
                dto.MainOtauPortDto = new OtauPortDto(otauLeaf.MasterPort, true)
                {
                    OtauId = rtu.MainVeexOtau.id,
                };
            }
            else // main
            {
                dto.OtauPortDto = new OtauPortDto(portNumber, true)
                {
                    OtauId = rtu.MainVeexOtau.id,
                };
                dto.MainOtauPortDto = null;
            }

            var toggleResult = await SendPreparationCommand(dto);
            return toggleResult ? new NetAddress(rtu.MainChannel.Ip4Address, 1500) : null;
        }

        private async Task<bool> ToggleToPort(Charon mainCharon, string serialOfCharonWithThisPort, int portNumber)
        {
            var result = mainCharon.SetExtendedActivePort(serialOfCharonWithThisPort, portNumber);
            if (result == CharonOperationResult.Ok)
                return true;
            var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{mainCharon.LastErrorMessage}");
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }

        private async Task<bool> SendPreparationCommand(PrepareReflectMeasurementDto dto)
        {
            RequestAnswer answer;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                answer = await _c2RWcfManager.PrepareReflectMeasurementAsync(dto);
            }
            if (answer.ReturnCode == ReturnCode.Ok)
                return true;

            var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{answer.ErrorMessage}");
            await _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;

        }


        public bool CanMeasurementClientAction(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;

            if (param is TraceLeaf traceLeaf && !traceLeaf.IsInZone) return false;

            var parent = GetParent(param);
            if (parent == null)
                return false;

            if (parent is OtauLeaf otauLeaf && otauLeaf.OtauState != RtuPartState.Ok)
                return false;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            return rtuLeaf.IsAvailable;
        }

        public bool CanMeasurementRftsReflectAction(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;

            if (param is TraceLeaf traceLeaf && !traceLeaf.IsInZone) return false;

            var parent = GetParent(param);
            if (parent == null)
                return false;

            if (parent is OtauLeaf otauLeaf && otauLeaf.OtauState != RtuPartState.Ok)
                return false;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            return rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

    }
}