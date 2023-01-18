using System;
using System.IO;
using Caliburn.Micro;
using Fibertest.Graph;
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
            DisplayName = "Fibertest 2.0 License maker";
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

        public void ToPdf()
        {
            // var provider = new PdfCertificateProvider();
            // var pdfDoc = provider.Create(LicenseInFileModel);
            // if (pdfDoc == null) return;
            // PdfExposer.Show(pdfDoc, @"LicenseCertificate.pdf", new WindowManager());
        }

        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}