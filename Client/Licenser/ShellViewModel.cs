using System;
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.Utils.Setup;
using iText.Html2pdf;
using Microsoft.Win32;
using WpfCommonViews;

namespace Licenser
{
    public class ShellViewModel : Screen, IShell
    {
        public bool HaveRights { get; set; }

        private bool _isEditable;
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                if (value == _isEditable) return;
                _isEditable = value;
                NotifyOfPropertyChange();
            }
        }

        private int _loadFromFileButtonRow;
        public int LoadFromFileButtonRow
        {
            get => _loadFromFileButtonRow;
            set
            {
                if (value == _loadFromFileButtonRow) return;
                _loadFromFileButtonRow = value;
                NotifyOfPropertyChange();
            }
        }

        private LicenseInFileModel _licenseInFileModel = new LicenseInFileModel();

        public LicenseInFileModel LicenseInFileModel
        {
            get => _licenseInFileModel;
            set
            {
                if (value == _licenseInFileModel) return;
                _licenseInFileModel = value;
                NotifyOfPropertyChange();
            }
        }

        public ShellViewModel()
        {
            var rr = Environment.GetCommandLineArgs();
            HaveRights = rr.Length > 1 && rr[1] == "ihaverights";

            if (rr.Length > 2)
            {
                var licFileDecoder = new LicenseFromFileDecoder(new WindowManager());
                var license = licFileDecoder.Decode(rr[2]).Result;
                if (license != null)
                    LicenseInFileModel = new LicenseInFileModel(license);
                IsEditable = true;
            }

            LoadFromFileButtonRow = IsEditable ? 0 : 1;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 3.0 License maker";
        }

        public void CreateNew()
        {
            LicenseInFileModel = new LicenseInFileModel()
            {
                LicenseId = Guid.NewGuid(),
                IsStandart = true,
                IsBasic = true,
                SecurityAdminPassword = Password.Generate(8),
                CreationDate = DateTime.Today,
            };
            IsEditable = true;
            LoadFromFileButtonRow = IsEditable ? 0 : 1;
        }

        public async void LoadFromFile()
        {
            var licenseFromFileDecoder = new LicenseFromFileDecoder(new WindowManager());
            var licFileReader = new LicenseFileChooser();
            var filename = licFileReader.ChooseFilename();
            if (filename == null) return;
            var license = await licenseFromFileDecoder.Decode(filename);

            if (license != null)
                LicenseInFileModel = new LicenseInFileModel(license);
            IsEditable = HaveRights;
            LoadFromFileButtonRow = IsEditable ? 0 : 1;
        }


        public void SaveAsFile()
        {
            if (LicenseInFileModel.IsIncremental)
                LicenseInFileModel.IsMachineKeyRequired = false;

            var license = LicenseInFileModel.ToLicenseInFile();
            var encoded = Cryptography.Encode(license);
            if (encoded == null) return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = LicenseInFileModel.LicenseKey; // Default file name
            dlg.DefaultExt = ".lic";
            dlg.Filter = "License file (*.lic)|*.lic";

            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                File.WriteAllBytes(filename, encoded);
            }
        }

        public void ToPdfButton()
        {
            var htmlContent = LicenseInFileModel.CreateHtmlReport();

            var reportFileName = FileOperations.GetMainFolder() + $@"\temp\Certificate-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.html";
            File.WriteAllText(reportFileName, htmlContent);
            Process.Start(new ProcessStartInfo() { FileName = reportFileName, UseShellExecute = true });

            var pdfFileName = iTextToPdf(htmlContent!);
            Process.Start(new ProcessStartInfo() { FileName = pdfFileName, UseShellExecute = true });
        }

        private string iTextToPdf(string htmlString)
        {
            var pdfFileName = FileOperations.GetMainFolder() + $@"\temp\Certificate-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.pdf";
            using var stream = new MemoryStream();
            HtmlConverter.ConvertToPdf(htmlString, stream, new ConverterProperties());
            File.WriteAllBytes(pdfFileName, stream.ToArray());
            return pdfFileName;
        }


        public async void CloseButton()
        {
            await TryCloseAsync();
        }
    }

}