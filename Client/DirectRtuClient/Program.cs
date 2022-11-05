using Fibertest.Dto;
using Grpc.Net.Client;
using Rtu;

namespace Fibertest.DirectRtuClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serverAddress = "localhost";
            // var serverAddress = "192.168.96.111";
            var uri = $"http://{serverAddress}:{(int)TcpPorts.RtuListenTo}";
            using var channel1 = GrpcChannel.ForAddress(uri);
            var grpcClient = new Greeter.GreeterClient(channel1);

            var helloRequest = new HelloRequest();
            HelloReply? response = await grpcClient.SayHelloAsync(helloRequest);
            Console.WriteLine(response?.Message ?? "response is null"); 
        }
    }
}