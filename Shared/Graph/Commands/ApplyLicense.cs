namespace Fibertest.Graph
{
    public class ApplyLicense
    {
        public Guid LicenseId;
        public Guid UserId; // user who sends license to server
        public bool IsIncremental; // by default = false -> Main license
        public string? Owner;

        public LicenseParameter RtuCount = new();
        public LicenseParameter ClientStationCount = new();
        public LicenseParameter WebClientCount = new();
        public LicenseParameter SuperClientStationCount = new();

        public bool IsMachineKeyRequired;
        public string? SecurityAdminPassword;
        public Guid AdminUserId;
        public string? MachineKey;

        public DateTime CreationDate; // Used in LicenseKey string
        public DateTime LoadingDate; // for evaluations
        public string? Version = @"2.0.0.0";
    }
}