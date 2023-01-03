using Fibertest.Dto;

namespace Fibertest.DataCenter;

public class RtuStation
{
    public int Id { get; set; }
    public Guid RtuGuid { get; set; }
    public string Version { get; set; } = "";
    public DateTime LastMeasurementTimestamp { get; set; }

    public string MainAddress { get; set; } = "";
    public int MainAddressPort { get; set; }
    public DateTime LastConnectionByMainAddressTimestamp { get; set; }
    public bool IsMainAddressOkDuePreviousCheck { get; set; }

    public bool IsReserveAddressSet { get; set; }
    public string ReserveAddress { get; set; } = "";
    public int ReserveAddressPort { get; set; }
    public DateTime LastConnectionByReserveAddressTimestamp { get; set; }
    public bool IsReserveAddressOkDuePreviousCheck { get; set; }

    public bool IsAvailable =>
        IsMainAddressOkDuePreviousCheck || (IsReserveAddressSet && IsReserveAddressOkDuePreviousCheck);

    public string? GetRtuAvailableAddress()
    {
        if (IsMainAddressOkDuePreviousCheck)
            return $"{MainAddress}:{MainAddressPort}";
        if (IsReserveAddressSet && IsReserveAddressOkDuePreviousCheck)
            return $"{ReserveAddress}:{ReserveAddressPort}";
        return null;
    }

    public DoubleAddress GetRtuDoubleAddress()
    {
        var rtuAddresses = new DoubleAddress()
        {
            Main = GetAddress(MainAddress, MainAddressPort),
            HasReserveAddress = IsReserveAddressSet,
        };
        if (rtuAddresses.HasReserveAddress)
            rtuAddresses.Reserve = GetAddress(ReserveAddress, ReserveAddressPort);
        return rtuAddresses;
    }

    private NetAddress GetAddress(string address, int port)
    {
        var netAddress = new NetAddress();
        if (NetAddress.IsValidIp4(address))
        {
            netAddress.Ip4Address = address;
            netAddress.IsAddressSetAsIp = true;
        }
        else
        {
            netAddress.HostName = address;
            netAddress.IsAddressSetAsIp = false;
        }
        netAddress.Port = port;
        return netAddress;
    }
}