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
}