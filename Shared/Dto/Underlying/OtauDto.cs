﻿namespace Fibertest.Dto;

[Serializable]
public class OtauDto
{
    public string? Serial;
    public string? OtauId;
    public NetAddress NetAddress = new NetAddress();
    public int OwnPortCount;
    public bool IsOk;
}