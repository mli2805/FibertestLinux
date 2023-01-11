using System.Net;
using System.Net.Sockets;

namespace Fibertest.Utils;

public static class LocalAddressResearcher
{
    public static string? GetLocalAddressToConnectServer(string serverAddress)
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect(serverAddress, 65530);
        IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
        return endPoint?.Address.ToString();
    }
        
    public static IEnumerable<string> GetAllLocalAddresses()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).Select(a => a.ToString());
    }
}