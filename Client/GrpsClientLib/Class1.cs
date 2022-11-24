using Fibertest.DataCenter;
using Fibertest.Dto;
using Fibertest.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StringResources;

namespace GrpsClientLib
{
    public class Class1
    {
        private readonly ILogger<Class1> _logger;

        public Class1(ILogger<Class1> logger)
        {
            _logger = logger;
        }

        public async Task<RtuInitializedDto?> F(string uri)
        {
            using var grpcChannel = GrpcChannel.ForAddress(uri);
            var grpcClient = new c2r.c2rClient(grpcChannel);
            return await InitDllsAndConnectOtdr(grpcClient);
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
        private async Task<RtuInitializedDto?> InitDllsAndConnectOtdr(c2r.c2rClient grpcClient)
        {
            var dto = new InitializeRtuDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
            var command = new c2rCommand()
                { Json = JsonConvert.SerializeObject(dto, JsonSerializerSettings) };
            _logger.Log(LogLevel.Information, Logs.Client.ToInt(), Resources.SID_long_operation_please_wait);

            try
            {
                var response = await grpcClient.SendCommandAsync(command);
                var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
                _logger.Log(LogLevel.Information, Logs.Client.ToInt(), Resources.SID_DllInit_result_is_ + (result == null ? "null" : $"{result.IsInitialized}"));
                if (result != null)
                    _logger.Log(LogLevel.Information, Logs.Client.ToInt(), Resources.SID_Serial_is__ + result.Serial);
                return result;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.Client.ToInt(), e.Message);
                return null;
            }
        }
    }

    public class Class2
    {
        private readonly ILogger<Class2> _logger;

        public Class2(ILogger<Class2> logger)
        {
            _logger = logger;
        }

        public int GetInt()
        {
                _logger.Log(LogLevel.Information, Logs.Client.ToInt(), "GetInt in Class2");
            return 4;
        }
    }
  public class Class3
    {
        public int GetInt()
        {
            return 4;
        }
    }


}