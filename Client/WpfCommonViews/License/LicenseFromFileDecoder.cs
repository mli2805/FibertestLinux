using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace WpfCommonViews
{
    public class LicenseFromFileDecoder
    {
        private readonly IWindowManager _windowManager;

        public LicenseFromFileDecoder(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public async Task<LicenseInFile?> Decode(string filename)
        {
            var encoded = File.ReadAllBytes(filename);
            try
            {
                return (LicenseInFile?)Cryptography.Decode(encoded);
            }
            catch (Exception e)
            {
                var lines = new List<string>()
                {
                    $"{Resources.SID_Invalid_license_file_}:", filename, "", e.Message
                };
                var mb = new MyMessageBoxViewModel(MessageType.Error, lines, 0);
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return null;
            }
        }
    }
}
