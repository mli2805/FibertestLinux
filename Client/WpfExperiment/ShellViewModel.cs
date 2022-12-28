using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WpfExperiment;

public class ShellViewModel : PropertyChangedBase, IShell
{
    private readonly ILogger<ShellViewModel> _logger;
    private readonly GrpcC2DRequests _grpcC2DRequests;
    private readonly GrpcC2RRequests _grpcC2RRequests;
    public string DcAddress { get; set; } = "192.168.96.21"; // virtualBox Ubuntu 20.04
    public string RtuAddress { get; set; } = "192.168.96.56"; // MAK 0068613

    private readonly string _clientId = "client-connection-id";
    private readonly string _username = "Vasya Pugovkin";
    private static readonly string _password = "123";
    private readonly string _clientIP = "192.168.96.21";

    public ObservableCollection<string> Lines { get; set; } = new() { " Here will be log " };

    public ShellViewModel(ILogger<ShellViewModel> logger, GrpcC2DRequests grpcC2DRequests, GrpcC2RRequests grpcC2RRequests)
    {
        _logger = logger;
        _grpcC2DRequests = grpcC2DRequests;
        _grpcC2DRequests.SetClientConnectionId(_clientId);
        _grpcC2RRequests = grpcC2RRequests;
        _grpcC2RRequests.SetClientConnectionId(_clientId);
    }

    public async void RegisterClient()
    {
        _grpcC2DRequests.ChangeAddress(DcAddress);
        var res = await _grpcC2DRequests
            .RegisterClient(new RegisterClientDto(_username, _password) { ClientIp = _clientIP });
        Lines.Add($"Client registered {res.ReturnCode == ReturnCode.ClientRegisteredSuccessfully}");
    }

    public async void AddRtu()
    {
        _grpcC2DRequests.ChangeAddress(DcAddress);
        var cmd = new AddRtuAtGpsLocation(Guid.NewGuid(), Guid.NewGuid(), 57.2, 29.6, "RTU linux");
        var res = await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        Lines.Add($"Add RTU: {res.ReturnCode}");
    }

    public async void InitializeRtu()
    {
        _grpcC2RRequests.ChangeAddress(DcAddress);
        _logger.Log(LogLevel.Information, Logs.Client.ToInt(), Resources.SID_long_operation_please_wait);
        var rtu = new Rtu() 
            { Id = Guid.NewGuid(), RtuMaker = RtuMaker.IIT, MainChannel = new NetAddress(RtuAddress, TcpPorts.RtuListenTo) };
        var dto = new InitializeRtuDto(rtu.Id, rtu.RtuMaker);
        var res = await _grpcC2RRequests.InitializeRtu(dto);
        if (res.ReturnCode == ReturnCode.RtuInitializedSuccessfully)
        {
            Lines.Add("RTU initialized successfully!");
            var json = JsonConvert.SerializeObject(res);
            Lines.Add(json);
        }
        else
        {
            Lines.Add($"Failed to initialize RTU. {res.ReturnCode} {res.ErrorMessage}");
        }
    }

    public async void Close()
    {
        await _grpcC2DRequests.UnRegisterClient(new UnRegisterClientDto(_username));
    }

}