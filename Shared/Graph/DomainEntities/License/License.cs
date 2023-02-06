namespace Fibertest.Graph;

[Serializable]
public class License
{
    public Guid LicenseId;
    public string LicenseKey => Lk();

    private string Lk()
    {
        var id = LicenseId.ToString().ToUpper().Substring(0, 8);
        var licType = IsIncremental ? @"I" : IsMachineKeyRequired ? @"BR" : @"BF";
        var stations = $@"{ClientStationCount.Value:D2}{WebClientCount.Value:D2}{SuperClientStationCount.Value:D2}";
        return $@"FT020-{id}-{licType}{RtuCount.Value:D2}{stations}-{CreationDate:yyMMdd}";
    }
        
    public bool IsIncremental; // by default = false -> Main license
    public string Owner = string.Empty;

    public LicenseParameter RtuCount = new();
    public LicenseParameter ClientStationCount = new();
    public LicenseParameter WebClientCount = new();
    public LicenseParameter SuperClientStationCount = new();

    public bool IsMachineKeyRequired;
    public string? SecurityAdminPassword;

    public DateTime CreationDate; // Used in LicenseKey string
    public DateTime LoadingDate; // for evaluations
    public string Version = @"2.0.0.0";

    public override string ToString()
    {
        return Lk();
    }
}