namespace Fibertest.Utils.Snmp;

// should be the same as in MIB file
public enum FtTrapType
{
    MeasurementAsSnmp = 100,
    RtuNetworkEventAsSnmp = 200,
    BopNetworkEventAsSnmp = 300,
    TestTrap = 777,
}