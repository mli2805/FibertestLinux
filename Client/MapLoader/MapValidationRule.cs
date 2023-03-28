using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using GMap.NET;

namespace MapLoader
{
    public class MapValidationRule : ValidationRule
    {
        bool _userAcceptedLicenseOnce;
        internal MainWindow? Window;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is OpenStreetMapProviderBase))
            {
                if (!_userAcceptedLicenseOnce)
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt"))
                    {
                        string ctn = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt");
                        int li = ctn.IndexOf("License", StringComparison.Ordinal);
                        string txt = ctn.Substring(li);

                        var d = new Message();
                        d.RichTextBox1.Text = txt;

                        if (true == d.ShowDialog())
                        {
                            _userAcceptedLicenseOnce = true;
                            if (Window != null)
                            {
                                Window.Title += " - license accepted by " + Environment.UserName + " at " + DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        // user deleted License.txt ;}
                        _userAcceptedLicenseOnce = true;
                    }
                }

                if (!_userAcceptedLicenseOnce)
                {
                    return new ValidationResult(false, "user do not accepted license ;/");
                }
            }

            return new ValidationResult(true, null);
        }
    }
}