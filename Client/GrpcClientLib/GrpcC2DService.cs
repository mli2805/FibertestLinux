using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fibertest.GrpcClientLib;

public class GrpcC2DService
{
    private readonly IWritableConfig<ClientConfig> _config;
    private readonly ILogger _logger;

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };

    private string ServerUri => $"http://{_config.Value.General.ServerAddress.Main.Ip4Address}:{(int)TcpPorts.ServerListenToCommonClient}";
    private string _clientConnectionId = "";

    public GrpcC2DService(IWritableConfig<ClientConfig> config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public void SetClientConnectionId(string clientConnectionId)
    {
        _clientConnectionId = clientConnectionId;
    }

    public async Task<TResult> SendAnyC2DRequest<T, TResult>(T dto) where T : BaseRequest where TResult : RequestAnswer, new()
    {
        return await SendAnyC2DRequest<T, TResult>(dto, ServerUri);
    }

    public async Task<RequestAnswer> CheckServerConnection(CheckServerConnectionDto dto, string addressToCheck)
    {
        var uriToCheck = $"http://{addressToCheck}:{(int)TcpPorts.ServerListenToCommonClient}";
        return await SendAnyC2DRequest<CheckServerConnectionDto, RequestAnswer>(dto, uriToCheck);
    }

    public async Task<RequestAnswer> SendEventSourcingCommand(object cmd)
    {
        var command = new c2dCommand { Json = JsonConvert.SerializeObject(cmd, JsonSerializerSettings), 
            IsEventSourcingCommand = true, ClientConnectionId = _clientConnectionId };

        return await SendAnyC2DRequest<RequestAnswer>(ServerUri, command);
    }

    private async Task<TResult> SendAnyC2DRequest<T, TResult>(T dto, string uri) where T : BaseRequest where TResult : RequestAnswer, new()
    {
        dto.ClientConnectionId = _clientConnectionId;
        var command = new c2dCommand { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        return await SendAnyC2DRequest<TResult>(uri, command);
    }

    private async Task<TResult> SendAnyC2DRequest<TResult>(string uri, c2dCommand command) where TResult : RequestAnswer, new()
    {
        using var grpcChannel = GrpcChannel.ForAddress(uri);
        var grpcClient = new c2d.c2dClient(grpcChannel);

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
}