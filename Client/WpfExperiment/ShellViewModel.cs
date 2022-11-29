﻿using System;
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
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly GrpcC2RRequests _grpcC2RRequests;
        public string DcAddress { get; set; } = "192.168.96.184"; // virtualBox Ubuntu 20.04
        public string RtuAddress { get; set; } = "192.168.96.56"; // MAK 0068613

        private readonly string _clientId = "client-connection-id";
        private readonly string _username = "Vasya Pugovkin";
        private readonly string _clientIP = "<<wpf address IP>>";

        public ShellViewModel(ILogger<ShellViewModel> logger, GrpcC2DRequests grpcC2DRequests, GrpcC2RRequests grpcC2RRequests)
        {
            _logger = logger;
            _grpcC2DRequests = grpcC2DRequests;
            _grpcC2DRequests.ChangeAddress(DcAddress);
            _grpcC2RRequests = grpcC2RRequests;
            _grpcC2RRequests.Initialize(DcAddress);
        }

        public async void RegisterClient()
        {
            var res = await _grpcC2DRequests.RegisterClient(new RegisterClientDto(_clientId) { UserName = _username, ClientIp = _clientIP });
            _logger.Log(LogLevel.Information, Logs.Client.ToInt(), $"Client registered {res.ReturnCode == ReturnCode.ClientRegisteredSuccessfully}");
        }

        public async void InitializeOtdr()
        {
            _logger.Log(LogLevel.Information, Logs.Client.ToInt(), Resources.SID_long_operation_please_wait);
            var rtu = new Rtu() { Id = Guid.NewGuid(), RtuMaker = RtuMaker.IIT, MainChannel = new NetAddress(RtuAddress, TcpPorts.RtuListenTo) };
            var dto = new InitializeRtuDto(_clientId, rtu.Id, rtu.RtuMaker);
            var res = await _grpcC2RRequests.InitializeRtu(dto);
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

        public async void Close()
        {
            await _grpcC2DRequests.UnRegisterClient(new UnRegisterClientDto(_clientId, _username));
        }

    }
}