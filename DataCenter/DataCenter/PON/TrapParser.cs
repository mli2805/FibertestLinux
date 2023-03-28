using System.Net;
using Fibertest.Graph;
using Fibertest.Utils;
using SnmpSharpNet;

namespace Fibertest.DataCenter
{
    public class TrapParser
    {
        private readonly ILogger<TrapParser> _logger;
        private readonly Model _writeModel;

        public TrapParser(ILogger<TrapParser> logger, Model writeModel)
        {
            _logger = logger;
            _writeModel = writeModel;
        }

        public TrapParserResult? ParseTrap(SnmpV2Packet pkt, EndPoint endPoint)
        {
            var ss = endPoint.ToString()!.Split(':');
            var tce = _writeModel.TcesNew.FirstOrDefault(o => o.Ip == ss[0]);
            if (tce == null)
            {
                _logger.Info(Logs.SnmpTraps, $"Unknown trap source address: {ss[0]}");
                return null;
            }

            if (!tce.ProcessSnmpTraps)
            {
                _logger.Info(Logs.SnmpTraps, $"Trap processing of {tce.Title} {tce.Ip} is turned off");
                return null;
            }

            var res = ParsePaket(pkt, tce);


            if (res != null)
                res.TceId = tce.Id;

            LogParsedTrap(res, tce);
            return res;
        }

        private void LogParsedTrap(TrapParserResult? trapParserResult, TceS tce)
        {
            if (trapParserResult != null)
            {
                var tcem = $"TCE: {tce.Title} {tce.Ip}";
                var slot = $"Slot: {trapParserResult.Slot}";
                var gpon = $"GponInterface: {trapParserResult.GponInterface}";
                var state = $"Trace state: {trapParserResult.State}";
                _logger.EmptyAndLog(Logs.SnmpTraps, $"{tcem} {slot} {gpon} {state}");
            }
            else
            {
                _logger.EmptyAndLog(Logs.SnmpTraps, $"Not a line event trap from {tce.Title} {tce.Ip}");
            }
        }

        private TrapParserResult? ParsePaket(SnmpV2Packet pkt, TceS tce)
        {
            switch (tce.TceTypeStruct.Code)
            {
                case @"Huawei_MA5600T_R016":
                    return pkt.ParseMa5600T_R016();
                case @"Huawei_MA5600T_R018":
                    return pkt.ParseMa5600T_R018();
                case @"Huawei_MA5608T_R013":
                    return pkt.ParseMa5608T_R013();

                case @"ZTE_C300_v1": return pkt.ParseC300();
                case @"ZTE_C300M_v4": return pkt.ParseC300M40();
                case @"ZTE_C300M_v43": return pkt.ParseC300M43();
                case @"ZTE_C320_v1": return pkt.ParseC320();
                default:
                    _logger.Info(Logs.SnmpTraps, $"Parser for OLT model {tce.TceTypeStruct.Code} is not implemented");
                    return null;
            }
        }
    }
}