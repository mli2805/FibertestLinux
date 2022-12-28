using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrpsClientLib;

public class GrpcC2DRequests
{
    private readonly ILogger<GrpcC2DRequests> _logger;

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private string _uri;
    private string _clientConnectionId = "";

    public GrpcC2DRequests(IConfiguration config, ILogger<GrpcC2DRequests> logger)
    {
        _logger = logger;
        var dcAddress = config.GetSection("General")["DcAddress"];
        _uri = $"http://{dcAddress}:{(int)TcpPorts.ServerListenToCommonClient}";
    }

    public void SetClientConnectionId(string clientConnectionId)
    {
        _clientConnectionId = clientConnectionId;
    }
    
    public void ChangeAddress(string dcAddress)
    {
        _uri = $"http://{dcAddress}:{(int)TcpPorts.ServerListenToCommonClient}";
    }

    public async Task<ClientRegisteredDto> RegisterClient(RegisterClientDto dto)
    {
        using var grpcChannel = GrpcChannel.ForAddress(_uri);
        var grpcClient = new c2d.c2dClient(grpcChannel);
        dto.ConnectionId = _clientConnectionId;
        var command = new c2dCommand()
            { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new ClientRegisteredDto(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "empty response" };

            var result = JsonConvert.DeserializeObject<ClientRegisteredDto>(response.Json);
            if (result == null)
                return new ClientRegisteredDto(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "json deserialization error" };

            return result;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.Client.ToInt(), e.Message);
            return new ClientRegisteredDto(ReturnCode.C2DGrpcOperationError) { ErrorMessage = e.Message };
        }
    }

    public async Task<RequestAnswer> SendEventSourcingCommand(object cmd)
    {
        using var grpcChannel = GrpcChannel.ForAddress(_uri);
        var grpcClient = new c2d.c2dClient(grpcChannel);
        var command = new c2dCommand()
            { Json = JsonConvert.SerializeObject(cmd, JsonSerializerSettings), IsEventSourcingCommand = true, ClientConnectionId = _clientConnectionId };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new ClientRegisteredDto(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "empty response" };

            var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json);
            if (result == null)
                return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "json deserialization error" };

            return result;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.Client.ToInt(), e.Message);
            return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = e.Message };
        }
    }

    public async Task<RequestAnswer> UnRegisterClient(UnRegisterClientDto dto)
    {
        using var grpcChannel = GrpcChannel.ForAddress(_uri);
        var grpcClient = new c2d.c2dClient(grpcChannel);
        dto.ConnectionId = _clientConnectionId;
        var command = new c2dCommand()
            { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new ClientRegisteredDto(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "empty response" };

            var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json);
            if (result == null)
                return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "json deserialization error" };

            return result;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.Client.ToInt(), e.Message);
            return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = e.Message };
        }
    }
}