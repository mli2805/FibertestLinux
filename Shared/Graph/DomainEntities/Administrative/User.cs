﻿using Fibertest.Dto;

namespace Fibertest.Graph;

public class User
{
    public Guid UserId;
    public string? Title;
    public string? EncodedPassword;
    public string? MachineKey;
    public EmailReceiver? Email;
    public SmsReceiver? Sms;
    public Role Role;
    public Guid ZoneId;

    public bool IsDefaultZoneUser => ZoneId == Guid.Empty;
}