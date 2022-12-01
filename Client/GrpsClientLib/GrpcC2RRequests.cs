using System.Diagnostics;
using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrpsClientLib;

public class GrpcC2RRequests
{
    private readonly ILogger<GrpcC2RRequests> _logger;
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    private string? _uri;

    public GrpcC2RRequests(IConfiguration config, ILogger<GrpcC2RRequests> logger)
    {
        _logger = logger;
        var d = config.GetSection("General")["Zoom"];
        Debug.WriteLine(d);
    }

    public void Initialize(string dcAddress)
    {
        _uri = $"http://{dcAddress}:{(int)TcpPorts.ServerListenToCommonClient}";
    }

    public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
    {
        if (_uri == null)
            return new RtuInitializedDto(ReturnCode.C2RGrpcOperationError) { ErrorMessage = "Data-center address not set" };

        using var grpcChannel = GrpcChannel.ForAddress(_uri);
        var grpcClient = new c2r.c2rClient(grpcChannel);

        var command = new c2rCommand()
            { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

        try
        {
            var response = await grpcClient.SendCommandAsync(command);
            if (response == null)
                return new RtuInitializedDto(ReturnCode.C2RGrpcOperationError) { ErrorMessage = "empty response" };

            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            if (result == null)
                return new RtuInitializedDto(ReturnCode.C2RGrpcOperationError) { ErrorMessage = "json deserialization error" };

            return result;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.Client.ToInt(), e.Message);
            return new RtuInitializedDto(ReturnCode.C2RGrpcOperationError) { ErrorMessage = e.Message };
        }
    }

}