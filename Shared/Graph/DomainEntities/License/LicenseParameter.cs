using Fibertest.StringResources;

namespace Fibertest.Graph;

[Serializable]
public class LicenseParameter
{
    public int Value;

    public DateTime ValidUntil;

    public LicenseParameter(){}

    public LicenseParameter(LicenseParameterInFile? licenseParameterInFile)
    {
        Value = licenseParameterInFile?.Value ?? 0;
        ValidUntil = licenseParameterInFile != null 
            ? licenseParameterInFile.IsTermInYears
                ? DateTime.Today.AddYears(licenseParameterInFile.Term)
                : DateTime.Today.AddMonths(licenseParameterInFile.Term) 
            : DateTime.Today.AddDays(-1);
    }

    public override string ToString()
    {
        return Value < 1 
            ? $@"{Resources.SID_no}" 
            : ValidUntil.Year > 2100
                ? $@"{Value}   ({Resources.SID_with_no_limitation_by_time}) "
                : $@"{Value}   ({Resources.SID_valid_until} {ValidUntil:dd MMMM yyyy}) ";
    }
}
