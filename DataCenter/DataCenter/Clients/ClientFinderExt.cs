using Fibertest.Dto;

namespace Fibertest.DataCenter;

public static class ClientFinderExt
{
    public static ClientStation? Get(this ClientCollection collection, string connectionId)
    {
        return !collection.Clients.TryGetValue(connectionId, out ClientStation? station) ? null : station;
    }

    public static List<DoubleAddress> GetAllDesktopClientsAddresses(this ClientCollection collection)
    {
        return collection.Clients.Values
            .Where(s => !s.IsWebClient)
            .Select(c => new DoubleAddress() { Main = new NetAddress(c.ClientIp, c.ClientAddressPort) })
            .ToList();
    }

    public static bool HasAnyWebClients(this ClientCollection collection)
    {
        return collection.Clients.Values.Any(s => s.IsWebClient);
    }

}