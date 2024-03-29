﻿using Fibertest.Dto;

namespace Fibertest.Graph;

[Serializable]
public class Measurement
{
    public DateTime MeasurementTimestamp;
    public DateTime EventRegistrationTimestamp { get; set; }
    public Guid RtuId;
    public Guid TraceId;
    public BaseRefType BaseRefType;
    public FiberState TraceState;

    public EventStatus EventStatus;
    public DateTime StatusChangedTimestamp;
    public string? StatusChangedByUser;

    public string? Comment;
    public List<AccidentOnTraceV2> Accidents = new();

    public int SorFileId { get; set; }
}