using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrpsClientLib;

public class GrpcC2RRequests
{
    private readonly IWritableConfig<ClientConfig> _config;
    private readonly ILogger _logger;
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    private string _uri => $"http://{_config.Value.General.ServerAddress.Main.Ip4Address}:{(int)TcpPorts.ServerListenToCommonClient}";
    private string _clientConnectionId = "";

    public GrpcC2RRequests(IWritableConfig<ClientConfig> config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public void SetClientConnectionId(string clientConnectionId)
    {
        _clientConnectionId = clientConnectionId;
    }

    public async Task<TResult> SendAnyC2RRequest<T, TResult>(T dto) where T : BaseRtuRequest where TResult : RequestAnswer, new()
    {
        dto.ClientConnectionId = _clientConnectionId;

        using var grpcChannel = GrpcChannel.ForAddress(_uri);
        var grpcClient = new c2r.c2rClient(grpcChannel);

        var command = new c2rCommand()
        { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new TResult
                {
                    ReturnCode = ReturnCode.C2RGrpcOperationError,
                    ErrorMessage = "Empty response",
                };

            var result = JsonConvert.DeserializeObject<TResult>(response.Json);
            if (result == null)
                return new TResult
                {
                    ReturnCode = ReturnCode.C2RGrpcOperationError,
                    ErrorMessage = "Client failed to deserialize response",
                };

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.Client, e.Message);
            return new TResult
            {
                ReturnCode = ReturnCode.C2RGrpcOperationError,
                ErrorMessage = e.Message,
            };
        }
    }

}