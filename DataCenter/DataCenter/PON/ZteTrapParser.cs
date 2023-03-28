using Fibertest.Dto;
using SnmpSharpNet;

namespace Fibertest.DataCenter
{
    public static class ZteTrapParser
    {
        public static TrapParserResult? ParseC320(this SnmpV2Packet pkt)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            if (ss.Length < 3 || (ss[2] != "eventLevel=critical" && ss[2] != "eventLevel=cleared")) return null;

            var pdu = pkt.Pdu[0];
            if (pdu.Oid.ToString() != "1.3.6.1.2.1.2.2.1.1") return null;

            var codeStr = pdu.Value.ToString();
            if (!int.TryParse(codeStr, out int code)) return null;

            var ee = ss[1].Split('=');
            var eventId = ss[2] == "eventLevel=critical" ? ee[1] : "";

            return CreateResult(@"ZTE_C320", ss[2], code, eventId);
        }

        public static TrapParserResult? ParseC300(this SnmpV2Packet pkt)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            if (ss.Length < 3 || (ss[2] != "eventLevel=critical" && ss[2] != "eventLevel=cleared")) return null;

            var pdu = pkt.Pdu[0];
            if (pdu.Oid.ToString() != "1.3.6.1.2.1.2.2.1.1") return null;

            var codeStr = pdu.Value.ToString();
            if (!int.TryParse(codeStr, out int code)) return null;

            var ee = ss[1].Split('=');
            var eventId = ss[2] == "eventLevel=critical" ? ee[1] : "";

            return CreateResult(@"ZTE_C300_v1", ss[2], code, eventId);
        }

        public static TrapParserResult? ParseC300M40(this SnmpV2Packet pkt)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            if (ss.Length < 3 || (ss[2] != "eventLevel=critical" && ss[2] != "eventLevel=cleared")) return null;

            var oid = pkt.Pdu[0].Oid.ToString();
            var point = oid.LastIndexOf('.');
            var oidMinus = oid.Substring(0, point);
            if (oidMinus != "1.3.6.1.4.1.3902.1082.500.10.2.2.3.1.1") return null;

            var codeStr = oid.Substring(point + 1);
            if (!int.TryParse(codeStr, out int code)) return null;

            var ee = ss[1].Split('=');
            var eventId = ss[2] == "eventLevel=critical" ? ee[1] : "";

            return CreateResult(@"ZTE_C300M_v4", ss[2], code, eventId);
        }

        public static TrapParserResult? ParseC300M43(this SnmpV2Packet pkt)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            if (ss.Length < 3 || (ss[2] != "eventLevel=critical" && ss[2] != "eventLevel=cleared" && ss[2] != "eventLevel=major")) return null;

            var oid = pkt.Pdu[0].Oid.ToString();
            var point = oid.LastIndexOf('.');
            var oidMinus = oid.Substring(0, point);
            if (oidMinus != "1.3.6.1.4.1.3902.1082.500.2.2.8.2.1.1") return null;

            var codeStr = oid.Substring(point + 1);
            if (!int.TryParse(codeStr, out int code)) return null;

            var ee = ss[1].Split('=');
            var eventId = (ss[2] == "eventLevel=critical" |ss[2] == "eventLevel=major") ? ee[1] : "";

            return CreateResult(@"ZTE_C300M_v4", ss[2], code, eventId);
        }

        /// <summary>
        /// zte code 0x03020100
        /// 
        /// zte c300 & c320 => on place 02 - slot; on place 01 - interface
        /// 
        /// zte c300M => on place 01 - slot; on place 00 - interface
        /// </summary>
        /// <param name="code">ZTE code 0x03020100</param>
        /// <param name="place">4 places in ZTE code (3 -> 0)</param>
        /// <returns></returns>
        public static int ExtractNumberFromZteCode(this int code, int place)
        {
            var shifted = code >> (place * 8);
            var result = shifted & 0x000000FF;
            return result;
        }

        private static TrapParserResult CreateResult(string tceTypeStructCode, string eventLevel, int code, string eventId = "")
        {
            var slot = tceTypeStructCode == "ZTE_C300M_v4" ? code.ExtractNumberFromZteCode(1) : code.ExtractNumberFromZteCode(2);
            var gponInterface = tceTypeStructCode == "ZTE_C300M_v4" ? code.ExtractNumberFromZteCode(0) : code.ExtractNumberFromZteCode(1);
            var result = new TrapParserResult
            {
                Slot = slot,
                GponInterface = gponInterface,
                State = eventLevel == "eventLevel=critical" ? FiberState.FiberBreak : FiberState.Ok,
                ZteEventId = eventId,
            };
            return result;
        }
    }
}