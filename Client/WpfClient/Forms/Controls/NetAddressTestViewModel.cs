using System;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class NetAddressTestViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly GrpcC2RRequests _grpcC2RRequests;
        private readonly NetAddressForConnectionTest _netAddressForConnectionTest;
        private bool? _result;
        private NetAddressInputViewModel _netAddressInputViewModel;

        public NetAddressInputViewModel NetAddressInputViewModel
        {
            get => _netAddressInputViewModel;
            set
            {
                if (Equals(value, _netAddressInputViewModel)) return;
                _netAddressInputViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonEnabled { get; set; }

        public bool? Result
        {
            get { return _result; }
            set
            {
                if (value == _result) return;
                _result = value;
                NotifyOfPropertyChange();
            }
        }

        public NetAddressTestViewModel(ILifetimeScope globalScope, CurrentUser currentUser, IWindowManager windowManager,
            GrpcC2DRequests grpcC2DRequests, GrpcC2RRequests grpcC2RRequests,
            NetAddressForConnectionTest netAddressForConnectionTest)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _windowManager = windowManager;
            _grpcC2DRequests = grpcC2DRequests;
            _grpcC2RRequests = grpcC2RRequests;
            _netAddressForConnectionTest = netAddressForConnectionTest;
            _netAddressInputViewModel = new NetAddressInputViewModel(netAddressForConnectionTest.Address, currentUser.Role <= Role.Root);
            IsButtonEnabled = currentUser.Role <= Role.Operator;
            Result = true;
        }

        public async void Test() // button
        {
            if (!NetAddressInputViewModel.IsValidIpAddress())
            {
                await _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return;
            }

            Result = null;
            bool res;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                res = await TestConnection(Guid.Empty, RtuMaker.IIT);
            }

            Result = res;
        }


        public async Task<bool> ExternalTest(Guid rtuId, RtuMaker rtuMaker) // from RTU initialization procedure
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                return await TestConnection(rtuId, rtuMaker);
            }
        }

        public bool IsValidIpAddress()
        {
            return NetAddressInputViewModel.IsValidIpAddress();
        }

        private async Task<bool> TestConnection(Guid rtuId, RtuMaker rtuMaker)
        {
            if (_netAddressForConnectionTest.IsRtuAddress)
            {
                var dto = new CheckRtuConnectionDto(rtuId, rtuMaker)
                {
                    NetAddress = NetAddressInputViewModel.GetNetAddress().Clone()
                };

                var resultDto = await _grpcC2RRequests.SendAnyC2RRequest<CheckRtuConnectionDto, RtuConnectionCheckedDto>(dto);
                if (resultDto.IsConnectionSuccessful && dto.NetAddress.Port != resultDto.NetAddress!.Port)
                {
                    NetAddressInputViewModel = 
                        new NetAddressInputViewModel(resultDto.NetAddress, _currentUser.Role <= Role.Root);
                }
                return resultDto.IsConnectionSuccessful;
            }
            else // DataCenter testing
            {
                var addressForTesting = new DoubleAddress()
                {
                    HasReserveAddress = false,
                    Main = NetAddressInputViewModel.GetNetAddress().Clone()
                };
                

               // var serverAnswer = await _grpcC2DRequests.SendAnyC2DRequest<CheckServerConnectionDto, RequestAnswer>(new CheckServerConnectionDto());
                var serverAnswer = await _grpcC2DRequests.CheckServerConnection(new CheckServerConnectionDto(), NetAddressInputViewModel.GetNetAddress().Ip4Address);
                return serverAnswer.ReturnCode == ReturnCode.Ok;
            }
        }
    }
}
