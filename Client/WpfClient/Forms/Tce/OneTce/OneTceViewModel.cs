﻿using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using Trace = Fibertest.Graph.Trace;

namespace Fibertest.WpfClient
{
    public class OneTceViewModel : Screen
    {
        private readonly CurrentUser _currentUser;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly TceReportProvider _tceReportProvider;
        private TceS _tceInWork = null!;
        public TceInfoViewModel TceInfoViewModel { get; set; } = new TceInfoViewModel();
        public TceSlotsViewModel TceSlotsViewModel { get; set; } = new TceSlotsViewModel();

        public bool IsSaveEnabled => !string.IsNullOrEmpty(TceInfoViewModel.Title) && _currentUser.Role <= Role.Root;

        public OneTceViewModel(CurrentUser currentUser, GrpcC2DService grpcC2DService,
            Model readModel, IWindowManager windowManager, TceReportProvider tceReportProvider)
        {
            _currentUser = currentUser;
            _grpcC2DService = grpcC2DService;
            _readModel = readModel;
            _windowManager = windowManager;
            _tceReportProvider = tceReportProvider;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Settings;
        }
        public void Initialize(TceS tce)
        {
            _tceInWork = tce;
            TceInfoViewModel.Initialize(tce, _currentUser.Role <= Role.Root);
            TceInfoViewModel.PropertyChanged += TceInfoViewModel_PropertyChanged;
            TceSlotsViewModel.Initialize(_readModel, tce, IsTraceLinked, _currentUser.Role <= Role.Root);
        }

        private void TceInfoViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Title")
                NotifyOfPropertyChange(nameof(IsSaveEnabled));
        }

        private bool IsTraceLinked(Trace trace)
        {
            var relation = _readModel.GponPortRelations.FirstOrDefault(r => r.TraceId == trace.TraceId);
            if (relation != null && relation.TceId != _tceInWork.Id)
                return true;

            return TceSlotsViewModel.Slots.Any(s => s.Gpons.Any(g => g.GponInWork.Trace?.TraceId == trace.TraceId));

            // не дает изменить связь если она существовала (т.к. у трассы уже выставлен флаг и не изменится пока не сохранишь удаление)
            //return trace.TraceToTceLinkState != TraceToTceLinkState.NoLink
            //       || TceSlotsViewModel.Slots.Any(s => s.Gpons.Any(g => g.GponInWork.Trace?.TraceId == trace.TraceId));
        }

        public void ExportToPdf()
        {
            var htmlContent = _tceReportProvider.Create(_tceInWork);
            if (htmlContent == null) return;

            var pdfFileName = htmlContent.SaveHtmlAsPdf($"TceReport_{_tceInWork.Title}");
            Process.Start(new ProcessStartInfo() { FileName = pdfFileName, UseShellExecute = true });
        }

        public async void ButtonSave()
        {
            await Save();
        }
        public async void ButtonSaveAndClose()
        {
            if (await Save())
                await TryCloseAsync();
        }

        private async Task<bool> Save()
        {
            if (! await Validate()) return false;

            var cmd = new AddOrUpdateTceWithRelations
            {
                Id = _tceInWork.Id,
                Title = TceInfoViewModel.Title,
                TceTypeStruct = _tceInWork.TceTypeStruct,
                Ip = TceInfoViewModel.Ip4InputViewModel.GetString(),
                Slots = TceSlotsViewModel.Slots.Select(s=>s.GetTceSlot()).ToList(),
                ProcessSnmpTraps = TceInfoViewModel.ProcessSnmpTraps,
                Comment = TceInfoViewModel.Comment,
                AllRelationsOfTce = TceSlotsViewModel.Slots.SelectMany(s => s.GetGponPortsRelations()).ToList(),
            };

            var result = await _grpcC2DService.SendEventSourcingCommand(cmd);
            if (result.ReturnCode != ReturnCode.Ok)
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, result.ErrorMessage!);
                await _windowManager.ShowDialogWithAssignedOwner(mb);
            }
            return result.ReturnCode == ReturnCode.Ok;
        }

        private async Task<bool> Validate()
        {
            if (_readModel.TcesNew.Any(t =>t.Title == TceInfoViewModel.Title && t.Id != _tceInWork.Id))
            {
                await _windowManager.ShowWindowWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_is_a_TCE_with_the_same_name));
                return false;
            }


            if (_readModel.TcesNew.Any(t => t.Ip == TceInfoViewModel.Ip4InputViewModel.GetString() && t.Id != _tceInWork.Id))
            {
                await _windowManager.ShowWindowWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_is_a_TCE_with_the_same_IP_address));
                return false;
            }
            return true;
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }

    }
}
