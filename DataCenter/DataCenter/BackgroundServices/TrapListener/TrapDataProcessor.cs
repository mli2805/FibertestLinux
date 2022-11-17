using System.Net;
using Fibertest.Utils;
using SnmpSharpNet;

namespace Fibertest.DataCenter;

public class TrapDataProcessor
{
    private readonly ILogger<TrapDataProcessor> _logger;

    public TrapDataProcessor(ILogger<TrapDataProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessData(byte[] inData, int inLen, EndPoint endPoint)
    {
        if (inLen > 0)
        {
            // Check protocol version int
            int ver = SnmpPacket.GetProtocolVersion(inData, inLen);
            if (ver == (int)SnmpVersion.Ver1)
            {
                _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"** SNMP Version 1 TRAP received from {endPoint}:");
                SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                pkt.decode(inData, inLen);
                LogSnmpVersion1TrapPacket(pkt);
            }
            else
            {
                _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"** SNMP Version 2 TRAP received from {endPoint}:");
                SnmpV2Packet pkt = new SnmpV2Packet();
                pkt.decode(inData, inLen);
                LogSnmpVersion2TrapPacket(pkt);
                await Task.Delay(1);
                //  await _trapExecutor.Process(pkt, endPoint, _logFile);
            }
        }
        else
        {
            if (inLen == 0)
                _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), "Zero length packet received.");
        }
    }

    private void LogSnmpVersion1TrapPacket(SnmpV1TrapPacket pkt)
    {
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** Trap generic: {pkt.Pdu.Generic}");
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** Trap specific: {pkt.Pdu.Specific}");
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** Agent address: {pkt.Pdu.AgentAddress}");
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** Timestamp: {pkt.Pdu.TimeStamp}");
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** VarBind count: {pkt.Pdu.VbList.Count}");
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), "*** VarBind content:");
        foreach (Vb v in pkt.Pdu.VbList)
        {
            _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
        }

        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"** End of SNMP Version 1 TRAP data." + Environment.NewLine);
    }

    private void LogSnmpVersion2TrapPacket(SnmpV2Packet pkt)
    {
        if (pkt.Pdu.Type != PduType.V2Trap)
        {
            _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** NOT an SNMPv2 trap ****");
        }
        else
        {
            _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** Community: {pkt.Community}");
            _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** System Up Time: {new TimeSpan(pkt.Pdu.TrapSysUpTime * 100000)}");
            _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** VarBind count: {pkt.Pdu.VbList.Count}");
            _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"*** VarBind content:");
            foreach (Vb v in pkt.Pdu.VbList)
            {
                _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
            }

            _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"** End of SNMP Version 2 TRAP data." + Environment.NewLine);
        }
    }
}