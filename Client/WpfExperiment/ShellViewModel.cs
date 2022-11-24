using System;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using StringResources;

namespace WpfExperiment
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        private readonly ILogger<ShellViewModel> _logger;
        private readonly GrpcClientRequests _grpcClientRequests;
        public string DcAddress { get; set; } = "192.168.96.109"; // virtualBox Ubuntu 20.04
        public string RtuAddress { get; set; } = "192.168.96.56"; // MAK 0068613

        private readonly string _clientId = "client-connection-id";

        public ShellViewModel(ILogger<ShellViewModel> logger, GrpcClientRequests grpcClientRequests)
        {
            _logger = logger;
            _grpcClientRequests = grpcClientRequests;
            _grpcClientRequests.Initialize(DcAddress);
        }

        public async void InitializeOtdr()
        {
            _logger.Log(LogLevel.Information, Logs.Client.ToInt(), Resources.SID_long_operation_please_wait);
            var rtu = new Rtu() { Id = Guid.NewGuid(), RtuMaker = RtuMaker.IIT, MainChannel = new NetAddress(RtuAddress, TcpPorts.RtuListenTo)};
            var dto = new InitializeRtuDto(_clientId, rtu.Id, rtu.RtuMaker);
            var res = await _grpcClientRequests.InitializeRtu(dto);
            if (res.ReturnCode == ReturnCode.RtuInitializedSuccessfully)
            {
                _logger.Log(LogLevel.Information, Logs.Client.ToInt(), "RTU initialized successfully!");
                _logger.Log(LogLevel.Information, Logs.Client.ToInt(), $"Serial: {res.Serial}");
            }
            else
            {
                _logger.Log(LogLevel.Error, Logs.Client.ToInt(), $"Failed to initialize RTU. {res.ReturnCode} {res.ErrorMessage}");
            }
        }
       
    }
}
