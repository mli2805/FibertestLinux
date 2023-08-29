using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public partial class Web2DCommandsProcessor
    {
        public async Task<string> GetTreeInJson(string username)
        {
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logger.Info(Logs.WebApi, "Not authorized access");
                return null;
            }
            await Task.Delay(1);

            try
            {
                var result = _writeModel.GetTree(_logger, user).ToList();
                var json = JsonConvert.SerializeObject(result, JsonSerializerSettings);
                // _logger.Info(Logs.WebApi, json, 0, 3);
                return json;
            }
            catch (Exception e)
            {
                _logger.Info(Logs.WebApi, $"{e.Message}");
                return "";
            }
        }

        public async Task<RtuInformationDto> GetRtuInformation(string username, Guid rtuId)
        {
            _logger.Info(Logs.WebApi, ":: WcfServiceForWebProxy GetRtuInformation");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logger.Info(Logs.WebApi, "Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new RtuInformationDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuTitle = rtu.Title;
            var rtuNode = _writeModel.Nodes.FirstOrDefault(n => n.NodeId == rtu.NodeId);
            result.GpsCoors = rtuNode?.Position.ToDetailedString(GpsInputMode.DegreesMinutesAndSeconds) ?? "";
            result.Comment = rtu.Comment;
            return result;
        }

        public async Task<RtuNetworkSettingsDto> GetRtuNetworkSettings(string username, Guid rtuId)
        {
            _logger.Info(Logs.WebApi, ":: WcfServiceForWebProxy GetRtuNetworkSettings");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logger.Info(Logs.WebApi, "Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new RtuNetworkSettingsDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuTitle = rtu.Title;
            result.RtuMaker = rtu.RtuMaker;
            result.Mfid = rtu.Mfid;
            result.Serial = rtu.Serial;
            result.Version = rtu.Version;
            result.Version2 = rtu.Version2;
            result.OwnPortCount = rtu.OwnPortCount;
            result.FullPortCount = rtu.FullPortCount;
            result.MainChannel = rtu.MainChannel.ToStringA();
            result.IsReserveChannelSet = rtu.IsReserveChannelSet;
            result.ReserveChannel = rtu.ReserveChannel.ToStringA();
            result.OtdrAddress = rtu.OtdrNetAddress.Ip4Address == "192.168.88.101"
                ? $"{rtu.MainChannel.Ip4Address}:{rtu.OtdrNetAddress.Port}"
                : rtu.OtdrNetAddress.ToStringA();
            return result;
        }

        public async Task<RtuStateDto?> GetRtuState(string username, Guid rtuId)
        {
            _logger.Info(Logs.WebApi, ":: WcfServiceForWebProxy GetRtuState");
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == username);
            if (user == null)
            {
                _logger.Info(Logs.WebApi, "Not authorized access");
                return null;
            }
            await Task.Delay(1);

            var result = new RtuStateDto();
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            if (rtu == null) return result;
            result.RtuId = rtu.Id.ToString();
            result.RtuTitle = rtu.Title;
            result.MainChannel = rtu.MainChannel.ToStringA();
            result.MainChannelState = rtu.MainChannelState;
            result.IsReserveChannelSet = rtu.IsReserveChannelSet;
            result.ReserveChannel = rtu.ReserveChannel.ToStringA();
            result.ReserveChannelState = rtu.ReserveChannelState;

            result.BopState = rtu.BopState;
            result.MonitoringMode = rtu.MonitoringState;

            result.OwnPortCount = rtu.OwnPortCount;
            result.FullPortCount = rtu.FullPortCount;
            result.BopCount = rtu.Children.Count;

            result.Children = PrepareRtuStateChildren(rtu);
            result.TraceCount = result.Children.Count(c => c.TraceState != FiberState.NotInTrace && c.TraceState != FiberState.Nothing);
            result.TracesState = result.Children.Max(c => c.TraceState);

            return result;
        }

        private List<RtuStateChildDto> PrepareRtuStateChildren(Graph.Rtu rtu)
        {
            var result = new List<RtuStateChildDto>();
            for (int i = 1; i <= rtu.OwnPortCount; i++)
            {
                if (rtu.Children.ContainsKey(i))
                {
                    var otau = rtu.Children[i];
                    for (int j = 1; j <= otau.OwnPortCount; j++)
                    {
                        var trace = _writeModel.Traces.FirstOrDefault(t =>
                            t.OtauPort != null && t.OtauPort.Serial == otau.Serial && t.OtauPort.OpticalPort == j);
                        result.Add(trace != null
                                   ? PrepareRtuStateChild(trace, j, $"{i}-")
                                   : new RtuStateChildDto() { Port = $"{i}-{j}", TraceState = FiberState.Nothing });
                    }
                }
                else
                {
                    var trace = _writeModel.Traces.FirstOrDefault(t =>
                        t.RtuId == rtu.Id && t.Port == i && (t.OtauPort == null || t.OtauPort.IsPortOnMainCharon));
                    result.Add(trace != null
                        ? PrepareRtuStateChild(trace, i, "")
                        : new RtuStateChildDto() { Port = i.ToString(), TraceState = FiberState.Nothing });
                }
            }
            return result;
        }

        private RtuStateChildDto PrepareRtuStateChild(Trace trace, int port, string mainPort)
        {
            var prepareRtuStateChild = new RtuStateChildDto()
            {
                Port = mainPort + port,
                TraceId = trace.TraceId.ToString(),
                TraceTitle = trace.Title,
                TraceState = trace.State,
                LastMeasId = _writeModel.Measurements.LastOrDefault(m => m.TraceId == trace.TraceId)?.SorFileId.ToString() ?? "",
                LastMeasTime = _writeModel.Measurements.LastOrDefault(m => m.TraceId == trace.TraceId)?.EventRegistrationTimestamp.ToString("G") ?? "",
            };
            return prepareRtuStateChild;
        }

        public Task<TreeOfAcceptableMeasParams> GetRtuAcceptableMeasParams(string username, Guid rtuId)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == rtuId);
            return Task.FromResult(rtu?.AcceptableMeasParams);
        }

    }
}
