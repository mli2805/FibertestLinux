// ReSharper disable InconsistentNaming

namespace Fibertest.Dto;

public class OtdrEngine
{
    public string? iit_otdr;
}

public class Other
{
    public string? os_info;
    public string? platform_firmware;
}

public class Components
{
    public string? api;
    public string? core;
    public string? httpServer;
    public OtdrEngine? otdrEngine = new OtdrEngine();
    public Other? other;
}

public class Platform
{
    public List<string>? enabledOptions;
    public string? firmwareVersion;
    public string? moduleFirmwareVersion;
    public string? name;
    public string? serialNumber;
}

public class VeexPlatformInfo
{
    public Components components = new Components();
    public DateTime dateTime;
    public string? id;
    public string? name;
    public Platform platform = new Platform();
}