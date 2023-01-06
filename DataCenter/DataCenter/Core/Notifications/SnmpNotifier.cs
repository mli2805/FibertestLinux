using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.Utils.Snmp;
using Microsoft.Extensions.Options;

namespace Fibertest.DataCenter
{
    public class SnmpNotifier
    {
        private readonly DataCenterConfig _config;
        private readonly ILogger<SnmpNotifier> _logger;
        private readonly Model _writeModel;
        private readonly SnmpAgent _snmpAgent;

        public SnmpNotifier(IOptions<DataCenterConfig> fullConfig, ILogger<SnmpNotifier> logger, Model writeModel, SnmpAgent snmpAgent)
        {
            _config = fullConfig.Value;
            _logger = logger;
            _writeModel = writeModel;
            _snmpAgent = snmpAgent;
        }

        public void SendTraceEvent(AddMeasurement meas)
        {
            var isSnmpOn = _config.Snmp.IsSnmpOn;
            if (!isSnmpOn) return;
            var data = MeasToSnmp(meas);

            _snmpAgent.SentRealTrap(data, FtTrapType.MeasurementAsSnmp);
            _logger.LLog(Logs.DataCenter.ToInt(), "SNMP trap sent");
        }

        public void SendRtuNetworkEvent(NetworkEvent rtuEvent)
        {
            var isSnmpOn = _config.Snmp.IsSnmpOn;
            if (!isSnmpOn) return;
            var data = RtuEventToSnmp(rtuEvent);

            _snmpAgent.SentRealTrap(data, FtTrapType.RtuNetworkEventAsSnmp);
        }

        public void SendBopNetworkEvent(AddBopNetworkEvent bopEvent)
        {
            var isSnmpOn = _config.Snmp.IsSnmpOn;
            if (!isSnmpOn) return;
            var data = BopEventToSnmp(bopEvent);

            _snmpAgent.SentRealTrap(data, FtTrapType.BopNetworkEventAsSnmp);

        }

        private List<KeyValuePair<FtTrapProperty, string>> MeasToSnmp(AddMeasurement meas)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == meas.RtuId)?.Title ?? "RTU not found";
            var traceTitle = _writeModel.Traces.FirstOrDefault(t => t.TraceId == meas.TraceId)?.Title ??
                             "Trace not found";

            var data = new List<KeyValuePair<FtTrapProperty, string>>
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventId, meas.SorFileId.ToString()),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventRegistrationTime,
                    meas.EventRegistrationTimestamp.ToString("G")),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuTitle, rtuTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.TraceTitle, traceTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.TraceState,
                  meas.TraceState.ToLocalizedString()),
            };
            foreach (var accident in meas.Accidents)
            {
                data.AddRange(AccidentToSnmp(accident));
            }

            return data;
        }

        private List<KeyValuePair<FtTrapProperty, string>> AccidentToSnmp(AccidentOnTraceV2 accident)
        {
            var accidentType = $"{accident.AccidentSeriousness.ToLocalizedString()} ({accident.OpticalTypeOfAccident.ToLetter()})";
            var data = new List<KeyValuePair<FtTrapProperty, string>>()
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentNodeTitle, accident.AccidentTitle ?? ""),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentType, accidentType),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentGps, accident.AccidentCoors.ToString()),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.AccidentToRtuDistanceKm,
                    accident.AccidentToRtuOpticalDistanceKm.ToString("0.000")),
            };
            if (accident.Left != null)
            {
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.LeftNodeTitle, accident.Left.Title ?? ""));
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.LeftNodeGps, accident.Left.Coors.ToString()));
                data.Add(new KeyValuePair<FtTrapProperty, string>(
                    FtTrapProperty.LeftNodeToRtuDistanceKm, accident.Left.ToRtuOpticalDistanceKm.ToString("0.000")));
            }
            if (accident.Right != null)
            {
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RightNodeTitle, accident.Right.Title ?? ""));
                data.Add(new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RightNodeGps, accident.Right.Coors.ToString()));
                data.Add(new KeyValuePair<FtTrapProperty, string>(
                    FtTrapProperty.RightNodeToRtuDistanceKm, accident.Right.ToRtuOpticalDistanceKm.ToString("0.000")));
            }

            return data;
        }

        private List<KeyValuePair<FtTrapProperty, string>> RtuEventToSnmp(NetworkEvent rtuEvent)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuEvent.RtuId)?.Title ?? "RTU not found";

            var data = new List<KeyValuePair<FtTrapProperty, string>>
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventRegistrationTime,
                    rtuEvent.EventTimestamp.ToString("G")),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuTitle, rtuTitle),
            };
            if (rtuEvent.OnMainChannel != ChannelEvent.Nothing)
                data.Add(
                    new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuMainChannel,
                        rtuEvent.OnMainChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken));
            if (rtuEvent.OnReserveChannel != ChannelEvent.Nothing)
                data.Add(
                    new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuReserveChannel,
                        rtuEvent.OnReserveChannel == ChannelEvent.Repaired ? Resources.SID_Recovered : Resources.SID_Broken));

            return data;
        }

        private List<KeyValuePair<FtTrapProperty, string>> BopEventToSnmp(AddBopNetworkEvent bopEvent)
        {
            var rtuTitle = _writeModel.Rtus.FirstOrDefault(r => r.Id == bopEvent.RtuId)?.Title ?? "RTU not found";
            var bopTitle =
                _writeModel.Otaus.FirstOrDefault(o => o.NetAddress.Ip4Address == bopEvent.OtauIp)?.NetAddress
                    .ToStringA() ?? "BOP not found";

            var data = new List<KeyValuePair<FtTrapProperty, string>>
            {
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.EventRegistrationTime,
                    bopEvent.EventTimestamp.ToString("G")),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.RtuTitle, rtuTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.BopTitle, bopTitle),
                new KeyValuePair<FtTrapProperty, string>(FtTrapProperty.BopState, bopEvent.IsOk
                    ? Resources.SID_Recovered : Resources.SID_Broken),
            };

            return data;
        }
    }
}
