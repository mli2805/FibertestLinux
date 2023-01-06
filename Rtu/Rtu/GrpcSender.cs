using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Fibertest.Rtu
{
    public class GrpcSender
    {
        private readonly IOptions<RtuConfig> _fullConfig;
        private readonly ILogger<GrpcSender> _logger;

        public GrpcSender(IOptions<RtuConfig> fullConfig, ILogger<GrpcSender> logger)
        {
            _fullConfig = fullConfig;
            _logger = logger;
        }

        public async Task SendToDc(string json)
        {
            var dcAddress = _fullConfig.Value.General.ServerAddress.Main.ToStringA();
            var uri = $"http://{dcAddress}";

            using var grpcChannel = GrpcChannel.ForAddress(uri);
            var grpcClient = new R2D.R2DClient(grpcChannel);
            var command = new R2DGrpcCommand() { Json = json };

            try
            {
                var response = await grpcClient.SendCommandAsync(command);
                if (response == null)
                {
                    _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "RTU failed to receive gRPC response");
                    return;
                }

                var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
                if (result == null)
                    _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "RTU failed to deserialize gRPC response");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.Client.ToInt(), "RTU failed to send gRPC message: " + e.Message);
            }
        }
    }
}
