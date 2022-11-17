namespace Fibertest.Graph
{
    
    public class LicenseInFile
    {
        public Guid LicenseId = Guid.NewGuid();
        public bool IsIncremental; // by default = false -> Main license
        public string? Owner;

        public LicenseParameterInFile RtuCount = new();
        public LicenseParameterInFile ClientStationCount = new();
        public LicenseParameterInFile WebClientCount = new();
        public LicenseParameterInFile SuperClientStationCount = new();

        public bool IsMachineKeyRequired;
        public byte[]? SecurityAdminPassword;
        public DateTime CreationDate; // Used in LicenseKey string
        public DateTime LoadingDate; // for evaluations
        public string? Version = @"2.0.0.0";

        public string Lk()
        {
            var id = LicenseId.ToString().ToUpper().Substring(0, 8);
            var licType = IsIncremental ? @"I" : IsMachineKeyRequired ? @"BR" : @"BF";
            var stations = $@"{ClientStationCount.Value:D2}{WebClientCount.Value:D2}{SuperClientStationCount.Value:D2}";
            return $@"FT020-{id}-{licType}{RtuCount.Value:D2}{stations}-{CreationDate:yyMMdd}";
        }
    }
}