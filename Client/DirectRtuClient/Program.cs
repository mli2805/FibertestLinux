using Fibertest.DataCenter;
using Fibertest.Dto;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace Fibertest.DirectRtuClient
{
    internal class Program
    {
        static async Task Main()
        {
            JsonSerializerSettings jsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
            
            var serverAddress = "localhost";
            // var serverAddress = "192.168.96.56";
            var uri = $"http://{serverAddress}:{(int)TcpPorts.ServerListenToCommonClient}";
            using var grpcChannel = GrpcChannel.ForAddress(uri);
            var grpcClient = new Server.ServerClient(grpcChannel);
            
            var dto = new InitializeRtuDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
            var command = new C2RTransferCommand() { Json = JsonConvert.SerializeObject(dto, jsonSerializerSettings) };
            C2RTransferResponse response = await grpcClient.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            Console.WriteLine("DllInit result is " + (result == null ? "null" : $"{result.IsInitialized}"));
            if (result != null)
                Console.WriteLine("Serial is: " + result.Serial);

            var dto2 = new FreeOtdrDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
            var command2 = new C2RTransferCommand() { Json = JsonConvert.SerializeObject(dto2, jsonSerializerSettings) };
            C2RTransferResponse response2 = await grpcClient.SendCommandAsync(command2);
            var result2 = JsonConvert.DeserializeObject<BaseRtuReply>(response2.Json);
            Console.WriteLine("FreeOtdr result is " + (result2 == null ? "null" : $"{result2.ReturnCode == ReturnCode.Ok}"));
            if (result2 != null)
                Console.WriteLine("FreeOtdr returned " + result2.ReturnCode);

            Console.ReadKey();
        }
    }
}