using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    public string DcAddress { get; set; } = "192.168.96.21"; // notebook
    // public string DcAddress { get; set; } = "192.168.96.184"; // virtualBox Ubuntu 20.04
    public string RtuAddress { get; set; } = "192.168.96.56"; // MAK 0068613

    private readonly string _clientId = "client-connection-id";
    private readonly string _username = "Vasya Pugovkin";
    private static readonly string _password = "123";
    private readonly string _clientIP = "192.168.96.21";

    private Guid _rtuId = Guid.Parse("87654321-f23f-441c-ae4b-84f25450c7e4");
    private Guid _otauId = Guid.Parse("12345678-f23f-441c-ae4b-84f25450c7e4");
    private Guid _rtuNodeId = Guid.Parse("34567890-f23f-441c-ae4b-84f25450c7e4");
    private Guid _traceId = Guid.Parse("5bb563ca-0ca1-466d-9c76-b60ba48133ef");

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
        var cmd = new AddRtuAtGpsLocation(_rtuId, _rtuNodeId, 57.2, 29.6, "RTU linux");
        var res = await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        _rtuNodeId = cmd.NodeId;
        Lines.Add($"Add RTU: {res.ReturnCode}");
    }

    public async void AddTrace()
    {
        _grpcC2DRequests.ChangeAddress(DcAddress);
        var cmd = new AddEquipmentAtGpsLocation(EquipmentType.Cross, 57.5, 29.9);
        var res = await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        if (res.ReturnCode != ReturnCode.Ok) return;
        var nodeId = cmd.NodeId;
        var equipmentId = cmd.RequestedEquipmentId;

        var cmd2 = new AddFiber(_rtuNodeId, nodeId);
        var res2 = await _grpcC2DRequests.SendEventSourcingCommand(cmd2);
        if (res2.ReturnCode != ReturnCode.Ok) return;
        var fiberId = cmd2.FiberId;
        
        var cmd3 = new AddTrace(_traceId, _rtuId, 
            new List<Guid>() {_rtuNodeId, nodeId}, 
            new List<Guid>(){_rtuId, equipmentId}, 
            new List<Guid>(){fiberId});
        var res3 = await _grpcC2DRequests.SendEventSourcingCommand(cmd3);
        Lines.Add($"Add trace: {res3.ReturnCode}");
    }

    public async void AttachTrace()
    {
        _grpcC2DRequests.ChangeAddress(DcAddress);
        var cmd = new AttachTrace(_traceId, new OtauPortDto(1, true, "68613"));
        var res = await _grpcC2DRequests.SendEventSourcingCommand(cmd);
        Lines.Add($"Attach trace: {res.ReturnCode}");
    }

    public async void AssignBaseRefs()
    {
        _grpcC2RRequests.ChangeAddress(DcAddress);
        var bytes = await File.ReadAllBytesAsync(@"c:\temp\sor\1 km 1 port.sor");
        var baseRefDtos = new List<BaseRefDto>()
        {
            new() { Id = Guid.NewGuid(), BaseRefType = BaseRefType.Precise, SorBytes = bytes},
            new() { Id = Guid.NewGuid(), BaseRefType = BaseRefType.Fast, SorBytes = bytes},
        };
        var dto = new AssignBaseRefsDto(_rtuId, RtuMaker.IIT, _traceId, baseRefDtos, new List<int>());
        dto.OtauPortDto = new OtauPortDto(1, true, "68613");
        var res = await _grpcC2RRequests.SendAnyC2RRequest<AssignBaseRefsDto, BaseRefAssignedDto>(dto);
        Lines.Add($"Assign Base Refs: {res.ReturnCode}");
    }

    public async void InitializeRtu()
    {
        _grpcC2RRequests.ChangeAddress(DcAddress);
        _logger.LogInfo(Logs.Client,Resources.SID_long_operation_please_wait);
        var dto = new InitializeRtuDto(_rtuId, RtuMaker.IIT);
        dto.RtuAddresses.Main = new NetAddress(RtuAddress, TcpPorts.RtuListenTo);

        var res = await _grpcC2RRequests.SendAnyC2RRequest<InitializeRtuDto, RtuInitializedDto>(dto);

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

    public async void AttachOtau()
    {
        _grpcC2RRequests.ChangeAddress(DcAddress);
        var dto = new AttachOtauDto(_rtuId, RtuMaker.IIT)
        {
            OtauId = _otauId,
            NetAddress = new NetAddress("192.168.96.57", TcpPorts.IitBop),
            OpticalPort = 4,
        };
        var res = await _grpcC2RRequests.SendAnyC2RRequest<AttachOtauDto, OtauAttachedDto>(dto);
        if (res.IsAttached)
        {
            _otauId = res.OtauId;
        }
    }

    public async void DetachOtau()
    {
        _grpcC2RRequests.ChangeAddress(DcAddress);
        var dto = new DetachOtauDto(_rtuId, RtuMaker.IIT)
        {
            OtauId = _otauId,
            NetAddress = new NetAddress("192.168.96.57", TcpPorts.IitBop),
            OpticalPort = 4,
        };
        var res = await _grpcC2RRequests.SendAnyC2RRequest<DetachOtauDto, OtauDetachedDto>(dto);
        if (res.IsDetached)
        {
            _otauId = Guid.Empty;
        }
    }

    public async void Close()
    {
        await _grpcC2DRequests.UnRegisterClient(new UnRegisterClientDto(_username));
    }

}