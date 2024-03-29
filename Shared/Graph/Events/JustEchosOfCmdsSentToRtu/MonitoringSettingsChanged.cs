﻿using Fibertest.Dto;

namespace Fibertest.Graph;

public class MonitoringSettingsChanged
{
    public Guid RtuId;

    public List<Guid> TracesInMonitoringCycle = new();

    public Frequency PreciseMeas;
    public Frequency PreciseSave;
    public Frequency FastSave;

    public bool IsMonitoringOn;
}