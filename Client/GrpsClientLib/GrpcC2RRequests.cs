using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrpsClientLib;

public class GrpcC2RRequests
{
    private readonly ILogger _logger;
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    private string? _uri;
    private string _clientConnectionId = "";

    public GrpcC2RRequests(IWritableConfig<ClientConfig> config, ILogger logger)
    {
        _logger = logger;
        var dcAddress = config.Value.General.ServerAddress.Main.Ip4Address;
        _uri = $"http://{dcAddress}:{(int)TcpPorts.ServerListenToCommonClient}";
    }

    public void SetClientConnectionId(string clientConnectionId)
    {
        _clientConnectionId = clientConnectionId;
    }

    public void ChangeAddress(string dcAddress)
    {
        _uri = $"http://{dcAddress}:{(int)TcpPorts.ServerListenToCommonClient}";
        _logger.LogInfo(Logs.Client, $"C2R gRPC service sends to {_uri}");
    }

    public async Task<TResult> SendAnyC2RRequest<T, TResult>(T dto) where T : BaseRtuRequest where TResult : RequestAnswer, new()
    {
        if (_uri == null)
            return new TResult
            {
                ReturnCode = ReturnCode.C2RGrpcOperationError,
                ErrorMessage = "Data-center address not set",
            };
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
            _logger.LogError(Logs.Client,e.Message);
            return new TResult
            {
                ReturnCode = ReturnCode.C2RGrpcOperationError,
                ErrorMessage = e.Message,
            };
        }
    }

}