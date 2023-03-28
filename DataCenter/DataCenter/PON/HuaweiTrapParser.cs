using Fibertest.Dto;
using SnmpSharpNet;

namespace Fibertest.DataCenter
{
    public static class HuaweiTrapParser
    {
        public static TrapParserResult? ParseMa5600T_R018(this SnmpV2Packet pkt)
        {
            if (pkt.Pdu.VbCount < 11) return null;
            var trapCode = pkt.Pdu[0].Value + pkt.Pdu[1].Value.ToString() + pkt.Pdu[2].Value;
            if (trapCode != "043") return null;

            return CreateResult(pkt.Pdu[6].Value.ToString()!, pkt.Pdu[7].Value.ToString()!, pkt.Pdu[11].Value.ToString()!);
        }

       public static TrapParserResult? ParseMa5600T_R016(this SnmpV2Packet pkt)
        {
            if (pkt.Pdu.VbCount < 10) return null;
            var trapCode = pkt.Pdu[0].Value + pkt.Pdu[1].Value.ToString() + pkt.Pdu[2].Value;
            if (trapCode != "043") return null;

            // 043 + 355 - is a problem with ONT modem, not with line!
            if (pkt.Pdu[4].Value.ToString() != "416") return null;

            return CreateResult(pkt.Pdu[6].Value.ToString()!, pkt.Pdu[7].Value.ToString()!, pkt.Pdu[11].Value.ToString()!);
        }

        public static TrapParserResult? ParseMa5608T_R013(this SnmpV2Packet pkt)
        {
            if (pkt.Pdu.VbCount < 12)
                return null; // not a fiber state trap
            var trapCode = pkt.Pdu[0].Value + pkt.Pdu[1].Value.ToString() + pkt.Pdu[2].Value;
            if (trapCode != "043") return null;

            // 043 + 355 - is a problem with ONT modem, not with line!
            if (pkt.Pdu[4].Value.ToString() != "416") return null;

            return CreateResult(pkt.Pdu[6].Value.ToString()!, pkt.Pdu[7].Value.ToString()!, pkt.Pdu[10].Value.ToString()!);
        }

        private static TrapParserResult CreateResult(string slotStr, string gponStr, string stateStr)
        {
            return new TrapParserResult()
            {
                Slot = int.Parse(slotStr),
                GponInterface = int.Parse(gponStr),
                State = stateStr == "2" ? FiberState.FiberBreak : FiberState.Ok,

            };
        }
    }
}