using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class TceTypeViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;
        private bool _isCreationMode;

        public TceTypeSelectionViewModel HuaweiSelectionViewModel { get; set; }
        public TceTypeSelectionViewModel ZteSelectionViewModel { get; set; }
        public Visibility ReSeedVisibility { get; set; }
        public int SelectedTabItem { get; set; }

        public TceTypeViewModel(Model readModel, GrpcC2DService grpcC2DService, IWindowManager windowManager, CurrentUser currentUser)
        {
            _readModel = readModel;
            _grpcC2DService = grpcC2DService;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Telecommunications_equipment_model;
        }

        public void Initialize(TceTypeStruct tceTypeStruct, bool isCreationMode)
        {
            _isCreationMode = isCreationMode;
            HuaweiSelectionViewModel = new TceTypeSelectionViewModel();
            HuaweiSelectionViewModel.Initialize(_readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.Huawei && s.IsVisible).ToList(), tceTypeStruct);
            ZteSelectionViewModel = new TceTypeSelectionViewModel();
            ZteSelectionViewModel.Initialize(_readModel.TceTypeStructs.Where(s => s.Maker == TceMaker.ZTE && s.IsVisible).ToList(), tceTypeStruct);

            ReSeedVisibility = _currentUser.Role <= Role.Root ? Visibility.Visible : Visibility.Collapsed;
        }

        public async Task ReSeed()
        {
            var cmd = new ReSeedTceTypeStructList() { TceTypes = TceTypeStructExt.Generate().ToList() };
            var res = await _grpcC2DService.SendEventSourcingCommand(cmd);
            if (res != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error,
                    @"Can't send Tce Types List!"));
            await TryCloseAsync(false);
        }

        public async void Select()
        {
            if (_isCreationMode)
            {
                await TryCloseAsync(true);
                return;
            }

            var vm = new MyMessageBoxViewModel(MessageType.Confirmation,
                new List<MyMessageBoxLineModel>()
                {
                    new MyMessageBoxLineModel(){ Line = Resources.SID_Changing_the_type_of_equipment_can_lead_to_loss_of_links, FontWeight = FontWeights.Bold },
                    new MyMessageBoxLineModel(){ Line = Resources.SID_RTU_port___telecommunication_equipment_interface, FontWeight = FontWeights.Bold },
                    new MyMessageBoxLineModel(){ Line = ""},
                    new MyMessageBoxLineModel(){ Line = ""},
                    new MyMessageBoxLineModel(){ Line = Resources.SID_Proceed_},
                });
            await _windowManager.ShowDialogWithAssignedOwner(vm);

           await TryCloseAsync(vm.IsAnswerPositive);
        }

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }
    }
}
