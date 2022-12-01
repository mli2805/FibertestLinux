﻿namespace Fibertest.Dto;

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
    RtuListenTo = 11942,
    RtuVeexListenTo = 80,

    // Client
    ClientListenTo = 11943, // when started under SuperClient: 11943 + _commandLineParameters.ClientOrdinal

    // SuperClient
    SuperClientListenTo = 11939,

    // БОП - IIT additional OTAU block
    IitBop = 11934,
}