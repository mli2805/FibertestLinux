using Fibertest.Dto;
using Grpc.Net.Client;
using Fibertest.Rtu;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            using var channel = GrpcChannel.ForAddress("http://localhost:11942");
            var client = new d2r.d2rClient(channel);

            var dto = new CheckRtuConnectionDto(Guid.Empty, RtuMaker.IIT);
            var json = JsonConvert.SerializeObject(dto, new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.All});
            var reply = await client.SendCommandAsync(new d2rCommand() { Json = json });
            Console.WriteLine($@"Server response: {reply!.Json}");
            Console.ReadKey();
        }
    }
}