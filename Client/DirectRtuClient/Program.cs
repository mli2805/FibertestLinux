using Fibertest.DataCenter;
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
            JsonSerializerSettings jsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
         
            
            var serverAddress = "localhost";
            // var serverAddress = "192.168.96.56";
            var uri = $"http://{serverAddress}:{(int)TcpPorts.ServerListenToCommonClient}";
            using var grpcChannel = GrpcChannel.ForAddress(uri);
            var grpcClient = new Server.ServerClient(grpcChannel);
            
            var dto = new InitializeRtuDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
            var command = new C2DGrpcCommand() { Json = JsonConvert.SerializeObject(dto, jsonSerializerSettings) };
            C2DGrpcResponse response = await grpcClient.SendCommandAsync(command);
            var result = JsonConvert.DeserializeObject<RtuInitializedDto>(response.Json);
            Console.WriteLine(result == null ? "response is null" : $"response is {result.IsInitialized}"); 
            
            // var rtuAddress = "localhost";
            // // var rtuAddress = "192.168.96.56";
            // var rtuUri = $"http://{rtuAddress}:{(int)TcpPorts.RtuListenTo}";
            // using var grpcChannelRtu = GrpcChannel.ForAddress(rtuUri);
            // var grpcClientRtu = new RtuManager.RtuManagerClient(grpcChannelRtu);
            //
            // var dto2 = new InitializeRtuDto("client-connection-id", Guid.NewGuid(), RtuMaker.IIT);
            // var command2 = new RtuGrpcCommand() { Json = JsonConvert.SerializeObject(dto2, jsonSerializerSettings) };
            // try
            // {
            //     RtuGrpcResponse response2 = await grpcClientRtu.SendCommandAsync(command2);
            //     var result2 = JsonConvert.DeserializeObject<RtuInitializedDto>(response2.Json);
            //     Console.WriteLine(result2 == null ? "RTU response is null" : $"RTU response is {result2.IsInitialized}");
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            // }

            Console.ReadKey();
        }
    }
}