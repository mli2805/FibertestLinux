using System.Globalization;
using Fibertest.DataCenter;
using Fibertest.Dto;
using Grpc.Net.Client;
using StringResources;

namespace ClientConsole
{
    internal class Program
    {
        static async Task Main()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");

            var serverAddress = "localhost";
            var uri = $"http://{serverAddress}:{(int)TcpPorts.ServerListenToCommonClient}";

            using var grpcChannel = GrpcChannel.ForAddress(uri);
            var grpcClient = new c2r.c2rClient(grpcChannel);

            while (true)
            {
                Console.WriteLine(Resources.SID_Example);
                Console.WriteLine("");
                if (!await Menu(grpcClient)) 
                    return;
            }
        }

        private static async Task<bool> Menu(c2r.c2rClient grpcClient)
        {
            Console.WriteLine(Resources.SID__1___InitDllsAndConnectOtdr);
            Console.WriteLine(Resources.SID__2___DisconnectOtdr);
            Console.WriteLine("");
            Console.WriteLine(Resources.SID__0___Exit);
            Console.WriteLine("");
            Console.WriteLine(Resources.SID_Choose_action___);
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