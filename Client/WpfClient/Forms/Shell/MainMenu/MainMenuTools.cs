using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Fibertest.Graph;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public partial class MainMenuViewModel
    {
        public async Task LaunchCleaningView()
        {
            var vm = _globalScope.Resolve<DbOptimizationViewModel>();
            await vm.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task LaunchGraphOptimizationView()
        {
            var vm = _globalScope.Resolve<GraphOptimizationViewModel>();
            await vm.Initialize();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void ImportRtuFromFolder()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var folder = Path.GetFullPath(Path.Combine(basePath, @"..\temp\"));
            string[] files = Directory.GetFiles(folder, @"*.brtu");

            foreach (var filename in files)
            {
                var bytes = File.ReadAllBytes(filename);
                var oneRtuModelFromFile = new Model();
                if (!await oneRtuModelFromFile.Deserialize(_logger, bytes)) return;

                await _modelFromFileExporter.Apply(oneRtuModelFromFile);
            }
        }

        public async void ExportEvents()
        {
           await _wcfDesktopC2D.ExportEvents();
        }

        public async void LaunchTcesView()
        {
            _tcesViewModel.Initialize();
            await _windowManager.ShowWindowWithAssignedOwner(_tcesViewModel);
        }
    }
}
