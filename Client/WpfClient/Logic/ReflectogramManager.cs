﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.OtdrDataFormat;
using Fibertest.Utils;
using Fibertest.Utils.Setup;
using Fibertest.WpfCommonViews;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Fibertest.WpfClient
{
    public class ReflectogramManager
    {
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly ILogger _logger;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IWindowManager _windowManager;

        private string _tempSorFile = string.Empty;

        public ReflectogramManager(IWritableConfig<ClientConfig> config, ILogger logger, 
             IWcfServiceCommonC2D c2DWcfCommonManager, IWindowManager windowManager)
        {
            _config = config;
            _logger = logger;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _windowManager = windowManager;
        }

        public void SetTempFileName(string traceTitle, int sorFileId, DateTime timestamp)
        {
            _tempSorFile = $@"{traceTitle} - ID{sorFileId} - {timestamp:dd-MM-yyyy-HH-mm-ss}.sor";
        }

        public void SetTempFileName(string traceTitle, string baseType, DateTime timestamp)
        {
            _tempSorFile = $@"{traceTitle} - {baseType} - {timestamp:dd-MM-yyyy-HH-mm-ss}.sor";
        }

        private string SaveInTempFolder(byte[] sorBytes)
        {
            var tempFolder = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory) + @"\Temp\";
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            var fullFilename = Path.Combine(tempFolder, _tempSorFile);
            File.WriteAllBytes(fullFilename, sorBytes);
            return fullFilename;
        }

        public async void ShowRefWithBase(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            var fullFilename = SaveInTempFolder(sorbytes);
            OpenSorInReflect(fullFilename);
        }

        public async void ShowOnlyCurrentMeasurement(int sorFileId)
        {
            byte[] sorbytesWithBase = await GetSorBytes(sorFileId);
            byte[] sorbytes = SorData.GetRidOfBase(sorbytesWithBase);
            var fullFilename = SaveInTempFolder(sorbytes);
            OpenSorInReflect(fullFilename);
        }

        public async void ShowBaseReflectogram(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            var fullFilename = SaveInTempFolder(sorbytes);
            OpenSorInReflect(fullFilename);
        }
        public async void ShowBaseReflectogramWithSelectedLandmark(int sorFileId, int lmNumber)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            var fullFilename = SaveInTempFolder(sorbytes);
            OpenSorInReflect(fullFilename, $@"-lm {lmNumber}");
        }

        public async void SaveReflectogramAs(int sorFileId, bool shouldBaseRefBeExcluded)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            SaveAsDlg(shouldBaseRefBeExcluded ? SorData.GetRidOfBase(sorbytes) : sorbytes);
        }

        public async void SaveBaseReflectogramAs(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            SaveAsDlg(sorbytes);
        }

        public void ShowClientMeasurement(byte[] sorBytes)
        {
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(clientPath + @"\temp"))
                Directory.CreateDirectory(clientPath + @"\temp");
            var filename = clientPath + $@"\temp\meas-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.sor";
            File.WriteAllBytes(filename, sorBytes);
            var iitPath = FileOperations.GetParentFolder(clientPath);
            Process.Start(iitPath + @"\RftsReflect\Reflect.exe", filename);
        }

        public async void ShowRftsEvents(int sorFileId, string traceTitle)
        {
            var sorbytes = await GetSorBytes(sorFileId);

            OtdrDataKnownBlocks? sorData;
            var result = SorData.TryGetFromBytes(sorbytes, out sorData);
            if (result != "")
            {
                _logger.LogInfo(Logs.Client, result);
                return;
            }

            var vm = new RftsEventsViewModel(_windowManager);
            vm.Initialize(sorData!, traceTitle);
            await _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        //------------------------------------------------------------------------------------------------
        public async Task<byte[]> GetSorBytes(int sorFileId)
        {
            var sorbytes = await _c2DWcfCommonManager.GetSorBytes(sorFileId);
            if (sorbytes == null)
            {
                _logger.LogError(Logs.Client, $@"Cannot get reflectogram for measurement {sorFileId}");
                return new byte[0];
            }
            return sorbytes;
        }

        private void OpenSorInReflect(string sorFilename, string options = "")
        {
            Process process = new Process();
            process.StartInfo.FileName = FileOperations.GetReflectInClient();
            process.StartInfo.Arguments = $@"{options} " + '"' + sorFilename + '"';
            process.Start();
        }

        private void SaveAsDlg(byte[] sorbytes)
        {
            // if InitialDirectory for OpenFileDialog does not exist:
            //   when drive in InitialDirectory exists - it's ok - will be used previous path from Windows
            //   but if even drive does not exist will be thrown exception
            var pathToSor = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory) + @"\tmp";

            // var initialDirectory = _iniFile.Read(IniSection.Miscellaneous, IniKey.PathToSor, pathToSor);
            var initialDirectory = _config.Value.Miscellaneous.PathToSor;
            if (!Directory.Exists(initialDirectory))
            {
                initialDirectory = pathToSor;
                _config.Update(c=>c.Miscellaneous.PathToSor = initialDirectory);
            }
            
            var sfd = new SaveFileDialog
            {
                Filter = @"Sor file (*.sor)|*.sor",
                InitialDirectory = initialDirectory,
                FileName = _tempSorFile,
            };
            if (sfd.ShowDialog() == true)
            {
                var path = Path.GetDirectoryName(sfd.FileName) ?? "";
                _config.Update(c=>c.Miscellaneous.PathToSor = path);
                File.WriteAllBytes(sfd.FileName, sorbytes);
            }
        }
    }
}
