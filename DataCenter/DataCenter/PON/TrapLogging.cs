using System.Net;
using Fibertest.Utils;
using SnmpSharpNet;

namespace Fibertest.DataCenter
{
    public static class TrapLogging
    {
        public static void LogSnmpVersion1TrapPacket(this ILogger<SnmpEngine2> logger, SnmpV1TrapPacket pkt, EndPoint endPoint)
        {
            logger.EmptyAndLog(Logs.SnmpTraps, $"** SNMP Version 1 TRAP received from {endPoint}:");
            logger.LogInfo(Logs.SnmpTraps, $"*** Trap generic: {pkt.Pdu.Generic}");
            logger.LogInfo(Logs.SnmpTraps, $"*** Trap specific: {pkt.Pdu.Specific}");
            logger.LogInfo(Logs.SnmpTraps, $"*** Agent address: {pkt.Pdu.AgentAddress}");
            logger.LogInfo(Logs.SnmpTraps, $"*** Timestamp: {pkt.Pdu.TimeStamp}");
            logger.LogInfo(Logs.SnmpTraps, $"*** VarBind count: {pkt.Pdu.VbList.Count}");
            logger.LogInfo(Logs.SnmpTraps, $"*** VarBind content:");
            foreach (Vb v in pkt.Pdu.VbList)
            {
                logger.LogInfo(Logs.SnmpTraps, $"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
            }

            logger.LogInfo(Logs.SnmpTraps, $"** End of SNMP Version 1 TRAP data.");
        }

        public static void LogSnmpVersion2TrapPacket(this ILogger<SnmpEngine2> logger, SnmpV2Packet pkt, EndPoint endPoint)
        {
            if (pkt.Pdu.Type != PduType.V2Trap)
            {
                logger.LogInfo(Logs.SnmpTraps, $"*** NOT an SNMPv2 trap ****");
            }
            else
            {
                logger.EmptyAndLog(Logs.SnmpTraps, $"** SNMP Version 2 TRAP received from {endPoint}:");
                logger.LogInfo(Logs.SnmpTraps, $"*** Community: {pkt.Community}");
                logger.LogInfo(Logs.SnmpTraps, $"*** System Up Time: {new TimeSpan(pkt.Pdu.TrapSysUpTime * 100000)}");
                logger.LogInfo(Logs.SnmpTraps, $"*** VarBind count: {pkt.Pdu.VbList.Count}");
                logger.LogInfo(Logs.SnmpTraps, $"*** VarBind content:");
                foreach (Vb v in pkt.Pdu.VbList)
                {
                    logger.LogInfo(Logs.SnmpTraps, $"**** {v.Oid} {SnmpConstants.GetTypeName(v.Value.Type)}: {v.Value}");
                }

                logger.LogInfo(Logs.SnmpTraps, $"** End of SNMP Version 2 TRAP data.");
            }
        }
    }
}