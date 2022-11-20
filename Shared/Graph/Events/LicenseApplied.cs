namespace Fibertest.Graph
{
    public class LicenseApplied
    {
        public Guid LicenseId;
        public Guid UserId; // user who sends license to server
        public bool IsIncremental; // by default = false -> Main license
        public string? Owner;
       
        public LicenseParameter? RtuCount;
        public LicenseParameter? ClientStationCount;
        public LicenseParameter? WebClientCount;
        public LicenseParameter? SuperClientStationCount;

        public bool IsMachineKeyRequired;
        public string? SecurityAdminPassword;
        public Guid AdminUserId;
        public string? MachineKey;

        public DateTime CreationDate; // Used in LicenseKey string
        public DateTime LoadingDate; // for evaluations
        public string? Version = @"2.0.0.0";
    }
}