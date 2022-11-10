using Fibertest.DataCenter;
using Fibertest.Dto;
using Grpc.Net.Client;

namespace ClientConsole
{
    internal class Program
    {
        static async Task Main()
        {
            var serverAddress = "localhost";
            var uri = $"http://{serverAddress}:{(int)TcpPorts.ServerListenToCommonClient}";

            using var grpcChannel = GrpcChannel.ForAddress(uri);
            var grpcClient = new c2r.c2rClient(grpcChannel);

            while (true)
            {
                Console.WriteLine("");
                Console.WriteLine("");
                if (!await Menu(grpcClient)) 
                    return;
            }
        }

        private static async Task<bool> Menu(c2r.c2rClient grpcClient)
        {
            Console.WriteLine("1 - InitDllsAndConnectOtdr");
            Console.WriteLine("2 - DisconnectOtdr");
            Console.WriteLine("");
            Console.WriteLine("0 - Exit");
            Console.WriteLine("");
            Console.WriteLine("Выберите действие - ");
            var l = Console.ReadLine();
            if (int.TryParse(l, out int action))
            {
                if (action == 0) return false;
                await Actions.Do(action, grpcClient);
            }
            return true;
        }
    }
}