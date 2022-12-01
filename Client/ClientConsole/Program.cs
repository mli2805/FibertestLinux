using System.Globalization;
using Fibertest.DataCenter;
using Fibertest.Dto;
using Grpc.Net.Client;
using StringResources;

namespace ClientConsole;

internal class Program
{
    static async Task Main()
    {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
        Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");

        // var serverAddress = "localhost";
        var serverAddress = "192.168.96.184";
        var uri = $"http://{serverAddress}:{(int)TcpPorts.ServerListenToCommonClient}";

        using var grpcChannel = GrpcChannel.ForAddress(uri);
        var grpcClient = new c2r.c2rClient(grpcChannel);
        var c2dClient = new c2d.c2dClient(grpcChannel);

        while (true)
        {
            Console.WriteLine(Resources.SID_Example);
            Console.WriteLine("");
            if (!await Menu(grpcClient, c2dClient)) 
                return;
        }
    }

    private static async Task<bool> Menu(c2r.c2rClient grpcClient, c2d.c2dClient c2dClient)
    {
        Console.WriteLine(Resources.SID__1___InitDllsAndConnectOtdr);
        Console.WriteLine(Resources.SID__2___DisconnectOtdr);
        Console.WriteLine(@"3 - Register Client");
        Console.WriteLine("");
        Console.WriteLine(Resources.SID__0___Exit);
        Console.WriteLine("");
        Console.WriteLine(Resources.SID_Choose_action___);
        var l = Console.ReadLine();
        if (int.TryParse(l, out int action))
        {
            if (action == 0) return false;
            await Actions.Do(action, grpcClient, c2dClient);
        }
        return true;
    }
}