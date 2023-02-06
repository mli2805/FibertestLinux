using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrpsClientLib;

public class GrpcC2DRequests
{
    private readonly IWritableConfig<ClientConfig> _config;
    private readonly ILogger _logger;

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private string ServerUri => $"http://{_config.Value.General.ServerAddress.Main.Ip4Address}:{(int)TcpPorts.ServerListenToCommonClient}";
    private string _clientConnectionId = "";

    public GrpcC2DRequests(IWritableConfig<ClientConfig> config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public void SetClientConnectionId(string clientConnectionId)
    {
        _clientConnectionId = clientConnectionId;
    }
    
    public async Task<ClientRegisteredDto> RegisterClient(RegisterClientDto dto)
    {
        using var grpcChannel = GrpcChannel.ForAddress(ServerUri);
        var grpcClient = new c2d.c2dClient(grpcChannel);
        dto.ClientConnectionId = _clientConnectionId;
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
            _logger.LogError(Logs.Client,e.Message);
            return new ClientRegisteredDto(ReturnCode.C2DGrpcOperationError) { ErrorMessage = e.Message };
        }
    }

    public async Task<RequestAnswer> SendEventSourcingCommand(object cmd)
    {
        using var grpcChannel = GrpcChannel.ForAddress(ServerUri);
        var grpcClient = new c2d.c2dClient(grpcChannel);
        var command = new c2dCommand { Json = JsonConvert.SerializeObject(cmd, JsonSerializerSettings), 
            IsEventSourcingCommand = true, ClientConnectionId = _clientConnectionId };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "empty response" };

            var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json);
            if (result == null)
                return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "json deserialization error" };

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.Client,e.Message);
            return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = e.Message };
        }
    }

    public async Task<RequestAnswer> UnRegisterClient(UnRegisterClientDto dto)
    {
        using var grpcChannel = GrpcChannel.ForAddress(ServerUri);
        var grpcClient = new c2d.c2dClient(grpcChannel);
        dto.ClientConnectionId = _clientConnectionId;
        var command = new c2dCommand()
            { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "empty response" };

            var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json);
            if (result == null)
                return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = "json deserialization error" };

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.Client,e.Message);
            return new RequestAnswer(ReturnCode.C2DGrpcOperationError) { ErrorMessage = e.Message };
        }
    }

    public async Task<TResult> SendAnyC2DRequest<T, TResult>(T dto) where T : BaseRequest where TResult : RequestAnswer, new()
    {
        using var grpcChannel = GrpcChannel.ForAddress(ServerUri);
        var grpcClient = new c2d.c2dClient(grpcChannel);

        dto.ClientConnectionId = _clientConnectionId;
        var command = new c2dCommand { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new TResult
                {
                    ReturnCode = ReturnCode.C2DGrpcOperationError,
                    ErrorMessage = "Empty response",
                };

            var result = JsonConvert.DeserializeObject<TResult>(response.Json);
            if (result == null)
                return new TResult
                {
                    ReturnCode = ReturnCode.C2DGrpcOperationError,
                    ErrorMessage = "Client failed to deserialize response",
                };

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.Client,e.Message);
            return new TResult
            {
                ReturnCode = ReturnCode.C2DGrpcOperationError,
                ErrorMessage = e.Message,
            };
        }
    }

    public async Task<RequestAnswer> CheckServerConnection(CheckServerConnectionDto dto, string addressToCheck)
    {
        ///////////////////////////////////////////////////////////
        var uriToCheck = $"http://{addressToCheck}:{(int)TcpPorts.ServerListenToCommonClient}";
        using var grpcChannel = GrpcChannel.ForAddress(uriToCheck);
        //////////////////////////////////////////////////////////////

        var grpcClient = new c2d.c2dClient(grpcChannel);

        dto.ClientConnectionId = _clientConnectionId;
        var command = new c2dCommand { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new RequestAnswer
                {
                    ReturnCode = ReturnCode.C2DGrpcOperationError,
                    ErrorMessage = "Empty response",
                };

            var result = JsonConvert.DeserializeObject<RequestAnswer>(response.Json);
            if (result == null)
                return new RequestAnswer
                {
                    ReturnCode = ReturnCode.C2DGrpcOperationError,
                    ErrorMessage = "Client failed to deserialize response",
                };

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.Client,e.Message);
            return new RequestAnswer
            {
                ReturnCode = ReturnCode.C2DGrpcOperationError,
                ErrorMessage = e.Message,
            };
        }
    }

}