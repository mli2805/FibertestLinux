namespace Fibertest.Graph;

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

      
}

public static class LicenseParameterExt
{
    // public static string ToString(this LicenseParameter lp)
    // {
    //     return lp.Value < 1 
    //         ? $@"{Resources.SID_no}" 
    //         : lp.ValidUntil.Year > 2100
    //             ? $@"{lp.Value}   ({Resources.SID_with_no_limitation_by_time}) "
    //             : $@"{lp.Value}   ({Resources.SID_valid_until} {lp.ValidUntil:dd MMMM yyyy}) ";
    // }
}