using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.Rtu
{
    public class GrpcR2DService
    {
        private readonly IWritableConfig<RtuConfig> _config;
        private readonly ILogger<GrpcR2DService> _logger;

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { TypeNameHandling = TypeNameHandling.All };
        private string ServerUri => $@"http://{_config.Value.General.ServerAddress.Main.Ip4Address}:{(int)TcpPorts.ServerListenToRtu}";

        public GrpcR2DService(IWritableConfig<RtuConfig> config, ILogger<GrpcR2DService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<TResult> SendAnyR2DRequest<T, TResult>(T dto)
            where T : BaseRequest where TResult : RequestAnswer, new()
        {
            try
            {
                var command = new R2DGrpcCommand() { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };

                if (!(dto is RtuChecksChannelDto))
                    _logger.Info(Logs.RtuService, $"{dto.What}");

                using var grpcChannelDc = GrpcChannel.ForAddress(ServerUri);
                var grpcClient = new R2D.R2DClient(grpcChannelDc);

                R2DGrpcResponse response = await grpcClient.SendCommandAsync(command);
                var result = JsonConvert.DeserializeObject<TResult>(response.Json);
                if (result == null)
                {
                    return (TResult)new RequestAnswer(ReturnCode.Error)
                    { ErrorMessage = "RTU failed deserialize DC response" };
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.Error(Logs.Client, "RTU failed to send gRPC message: " + e.Message);
                var result = new RequestAnswer(ReturnCode.Error) { ErrorMessage = e.Message };
                return (TResult)result;
            }
        }

    }
}
