using Microsoft.Win32;

namespace WpfCommonViews
{
    public interface ILicenseFileChooser
    {
        string? ChooseFilename(string initialDirectory = "");
    }

    public class LicenseFileChooser : ILicenseFileChooser
    {
        public string? ChooseFilename(string initialDirectory = "")
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = @".lic";
            dlg.InitialDirectory = initialDirectory;
            dlg.Filter = @"License file  |*.lic";
            if (dlg.ShowDialog() != true) return null;

            return dlg.FileName;
        }
    }
}