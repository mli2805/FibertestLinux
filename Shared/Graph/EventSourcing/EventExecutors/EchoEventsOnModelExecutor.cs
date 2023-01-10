using Fibertest.Dto;

namespace Fibertest.Graph;

public static class EchoEventsOnModelExecutor
{
       
    public static string? AssignBaseRef(this Model model, BaseRefAssigned e)
    {
        var trace = model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
        if (trace == null)
        {
            return $@"BaseRefAssigned: Trace {e.TraceId} not found";
        }

        var preciseBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
        if (preciseBaseRef != null)
        {
            var oldBaseRef = model.BaseRefs.FirstOrDefault(b =>
                b.TraceId == preciseBaseRef.TraceId && b.BaseRefType == preciseBaseRef.BaseRefType);
            if (oldBaseRef != null)
                model.BaseRefs.Remove(oldBaseRef);
            if (preciseBaseRef.Id != Guid.Empty)
                model.BaseRefs.Add(preciseBaseRef);
            trace.PreciseId = preciseBaseRef.Id;
            trace.PreciseDuration = preciseBaseRef.Duration;
        }
        var fastBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
        if (fastBaseRef != null)
        {
            var oldBaseRef = model.BaseRefs.FirstOrDefault(b =>
                b.TraceId == fastBaseRef.TraceId && b.BaseRefType == fastBaseRef.BaseRefType);
            if (oldBaseRef != null)
                model.BaseRefs.Remove(oldBaseRef);
            if (fastBaseRef.Id != Guid.Empty)
                model.BaseRefs.Add(fastBaseRef);
            trace.FastId = fastBaseRef.Id;
            trace.FastDuration = fastBaseRef.Duration;
        }
        var additionalBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
        if (additionalBaseRef != null)
        {
            var oldBaseRef = model.BaseRefs.FirstOrDefault(b =>
                b.TraceId == additionalBaseRef.TraceId && b.BaseRefType == additionalBaseRef.BaseRefType);
            if (oldBaseRef != null)
                model.BaseRefs.Remove(oldBaseRef);
            if (additionalBaseRef.Id != Guid.Empty)
                model.BaseRefs.Add(additionalBaseRef);
            trace.AdditionalId = additionalBaseRef.Id;
            trace.AdditionalDuration = additionalBaseRef.Duration;
        }
        if (!trace.HasEnoughBaseRefsToPerformMonitoring)
            trace.IsIncludedInMonitoringCycle = false;

        return null;
    }

    public static string? InitializeRtu(this Model model, RtuInitialized e)
    {
        var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.Id);
        if (rtu == null)
        {
            return $@"RtuInitialized: RTU {e.Id.First6()} not found";
        }

        if (rtu.Serial == e.Serial && !IsOtauStructureValid(rtu, e))
        {
            return $@"RtuInitialized: Otau structure does match";
        }

        if (e.OtauNetAddress == null)
            return null;

        model.SetRtuProperties(rtu, e);
        return null;
    }

    private static bool IsOtauStructureValid(Rtu rtu, RtuInitialized e)
    {
        foreach (var keyValuePair in rtu.Children)
        {
            if (!e.Children.ContainsKey(keyValuePair.Key))
                return false;
        }

        return true;
    }


    private static void SetRtuProperties(this Model model, Rtu rtu, RtuInitialized e)
    {
        rtu.RtuMaker = e.Maker;

        rtu.MainVeexOtau = e.MainVeexOtau;
        rtu.OtdrId = e.OtdrId;

        rtu.Mfid = e.Mfid;
        rtu.Mfsn = e.Mfsn;
        rtu.Omid = e.Omid;
        rtu.Omsn = e.Omsn;

        if (rtu.Serial != e.Serial)
        {
            foreach (Trace trace in model.Traces.Where(t => t.OtauPort != null && t.OtauPort.Serial == rtu.Serial))
            {
                if (trace.OtauPort != null) // nonsense but but it is a Resharper's fault
                    trace.OtauPort.Serial = e.Serial!;
            }

            // if in Client OTAU attached to not existent now port
            // RTU removed it from charon ini, won't return it and Client should remove it
            foreach (var otau in model.Otaus.Where(o=> o.RtuId == rtu.Id && !o.IsMainOtau && o.MasterPort >= rtu.OwnPortCount).ToList())
            {
                rtu.Children.Remove(otau.MasterPort);
                model.Otaus.Remove(otau);
            }

            rtu.Serial = e.Serial;
        }

        rtu.OwnPortCount = e.OwnPortCount;
        rtu.FullPortCount = e.OwnPortCount;
        foreach (var pair in rtu.Children)
        {
            rtu.FullPortCount += pair.Value.IsOk 
                ? pair.Value.OwnPortCount 
                : rtu.Children[pair.Key].OwnPortCount;
        }

        rtu.MainChannel = e.MainChannel ?? new NetAddress("", -1);
        rtu.MainChannelState = e.MainChannelState;
        rtu.IsReserveChannelSet = e.IsReserveChannelSet;
        if (e.IsReserveChannelSet)
            rtu.ReserveChannel = e.ReserveChannel ?? new NetAddress("", -1);
        rtu.ReserveChannelState = e.ReserveChannelState;
        rtu.OtdrNetAddress = e.OtauNetAddress ?? new NetAddress("", -1);
        rtu.Version = e.Version;
        rtu.Version2 = e.Version2;
        rtu.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
        rtu.AcceptableMeasParams = e.AcceptableMeasParams;
          
        if (rtu.RtuMaker == RtuMaker.VeEX)
        {
            var mainVeexOtau = model.Otaus.FirstOrDefault(o => o.RtuId == rtu.Id && o.VeexRtuMainOtauId == rtu.MainVeexOtau.id);
            if (mainVeexOtau == null)
            {
                mainVeexOtau = new Otau()
                {
                    VeexRtuMainOtauId = rtu.MainVeexOtau.id,
                    RtuId = rtu.Id,
                    IsMainOtau = true,
                    NetAddress = e.OtauNetAddress ?? new NetAddress(),
                    PortCount = rtu.OwnPortCount,
                    IsOk = true,
                };
                model.Otaus.Add(mainVeexOtau);
            }

            foreach (var trace in model.Traces.Where(t=>t.RtuId == rtu.Id))
            {
                if (trace.OtauPort != null && trace.OtauPort.IsPortOnMainCharon && trace.OtauPort.OtauId == null)
                    trace.OtauPort.OtauId = mainVeexOtau.VeexRtuMainOtauId;
            }
        }

        /*
                RTU cannot return child OTAU which does not exist yet! It's a business rule
                Client sends existing OTAU list -> 
                RTU MUST detach any OTAU which are not in client's list
                and attach all OTAU from this list

         
        */
        foreach (var childPair in e.Children)
        {
            var otau = model.Otaus.FirstOrDefault(o => o.NetAddress.Equals(childPair.Value.NetAddress));
            if (otau != null)
            {
                otau.IsOk = childPair.Value.IsOk;
                rtu.SetOtauState(otau.Id, otau.IsOk);
            }
                
            if (rtu.Children.TryGetValue(childPair.Key, out OtauDto? otauDto))
            {
                otauDto.IsOk = childPair.Value.IsOk;
            }
        }
    }

    public static string? ChangeMonitoringSettings(this Model model, MonitoringSettingsChanged e)
    {
        var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
        if (rtu == null)
        {
            return $@"MonitoringSettingsChanged: RTU {e.RtuId.First6()} not found";
        }
        rtu.PreciseMeas = e.PreciseMeas;
        rtu.PreciseSave = e.PreciseSave;
        rtu.FastSave = e.FastSave;
        rtu.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;

        foreach (var trace in model.Traces.Where(t => t.RtuId == e.RtuId))
        {
            trace.IsIncludedInMonitoringCycle = e.TracesInMonitoringCycle.Contains(trace.TraceId);
        }
        return null;
    }

    public static string? StartMonitoring(this Model model, MonitoringStarted e)
    {
        var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
        if (rtu == null)
        {
            return $@"MonitoringStarted: RTU {e.RtuId.First6()} not found";
        }
        rtu.MonitoringState = MonitoringState.On;
        return null;
    }

    public static string? StopMonitoring(this Model model, MonitoringStopped e)
    {
        var rtu = model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
        if (rtu == null)
        {
            return $@"MonitoringStopped: RTU {e.RtuId.First6()} not found";
        }
        rtu.MonitoringState = MonitoringState.Off;
        return null;
    }


}