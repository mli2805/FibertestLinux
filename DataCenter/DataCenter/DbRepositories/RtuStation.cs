namespace Fibertest.DataCenter;

public class RtuStation
{
    public int Id;
    public Guid RtuGuid;
    public string Version = "";
    public DateTime LastMeasurementTimestamp;

    public string MainAddress = "";
    public int MainAddressPort;
    public DateTime LastConnectionByMainAddressTimestamp;
    public bool IsMainAddressOkDuePreviousCheck;

    public bool IsReserveAddressSet;
    public string ReserveAddress = "";
    public int ReserveAddressPort;
    public DateTime LastConnectionByReserveAddressTimestamp;
    public bool IsReserveAddressOkDuePreviousCheck;

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
}