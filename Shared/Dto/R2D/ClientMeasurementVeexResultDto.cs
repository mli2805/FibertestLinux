﻿namespace Fibertest.Dto;

public class ClientMeasurementVeexResultDto : RequestAnswer
{
    public string? VeexMeasurementStatus;
    public List<ConnectionQuality>? ConnectionQuality;
        
    public byte[]? SorBytes;
}