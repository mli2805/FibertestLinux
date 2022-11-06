using Fibertest.Dto;
using Fibertest.Rtu;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.DirectRtuClient
{
    internal class Program
    {
        static async Task Main()
        {
            var serverAddress = "localhost";
            // var serverAddress = "192.168.96.56";
            var uri = $"http://{serverAddress}:{(int)TcpPorts.RtuListenTo}";
            using var grpcChannel = GrpcChannel.ForAddress(uri);
            var grpcClient = new RtuManager.RtuManagerClient(grpcChannel);

            var dto = new InitializeRtuDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
            var command = new RtuGrpcCommand() { Json = JsonConvert.SerializeObject(dto) };
            RtuGrpcResponse response = await grpcClient.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            Console.WriteLine(result == null ? "response is null" : $"response is {result.IsInitialized}");
        }
    }
}