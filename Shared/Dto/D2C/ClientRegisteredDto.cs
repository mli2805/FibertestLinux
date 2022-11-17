﻿namespace Fibertest.Dto
{
    public class ClientRegisteredDto : BaseReply
    {
        public string? ConnectionId;

        public Guid UserId;
        public Role Role;
        public Guid ZoneId;
        public string? ZoneTitle;
        
        public Guid StreamIdOriginal;
        public int SnapshotLastEvent;
        public DateTime SnapshotLastDate;
        public string? DatacenterVersion;
        public bool IsWithoutMapMode;
        
        public SmtpSettingsDto? Smtp;
        public int GsmModemComPort;
        public SnmpSettingsDto? Snmp;
    }
}