﻿namespace Fibertest.Dto;

public class VeexTestCreatedDto
{
    public Guid TestId;
    public Guid TraceId;
    public BaseRefType BasRefType;
    
    public bool IsOnBop;
    public string? OtauId;
    
    public DateTime CreationTimestamp;
}