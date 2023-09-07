namespace Fibertest.Dto;

public enum TcpPorts
{
    // DataCenterService
    ServerListenToDesktopClient = 11940,
    ServerListenToWebClient = 11938,
    ServerListenToCommonClient = 11937,

    ServerListenToRtu = 11941,

    // DataCenterWebApi
    WebApiListenTo = 11080,

    // RTU
    RtuListenToGrpc = 11942,
    RtuVeexListenTo = 80,
    RtuListenToHttp = 11900,

    // Client
    ClientListenTo = 11943, // when started under SuperClient: 11943 + _commandLineParameters.ClientOrdinal

    // SuperClient
    SuperClientListenTo = 11939,

    // БОП - IIT additional OTAU block
    // hardcoded in mikrotik ?
    IitBop = 11834,
}