using System.Globalization;
using Fibertest.StringResources;
using Fibertest.Utils;

namespace Fibertest.Graph;

[Serializable]
public class LicenseParameterInFile
{
    public int Value = 1;

    public int Term = 3;
    public bool IsTermInYears; // Or in months

    public override string ToString()
    {
        if (Value <= 0) return "";
        string term;
        if (Term == 999 && IsTermInYears)
        {
            term = Resources.SID_with_no_limitation_by_time;
        }
        else
        {
            var yearInLocalLng = Thread.CurrentThread.CurrentUICulture.Equals(new CultureInfo("ru-RU"))
                ? Value.GetYearInRussian()
                : "year(s)";
            var units = IsTermInYears ? yearInLocalLng : Resources.SID_month_s_;
            term = string.Format(Resources.SID_for__0___1_, Term, units);
        }

        return $"{Value} ({term})";
    }
}