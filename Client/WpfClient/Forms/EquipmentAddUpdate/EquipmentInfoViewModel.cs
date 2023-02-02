using System;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class EquipmentInfoViewModel : Screen
    {
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }
        public Guid NodeId;
        private ViewMode _mode;
        private readonly IWritableConfig<ClientConfig> _config;
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private readonly IWindowManager _windowManager;

        public EquipmentInfoModel Model { get; set; } = new EquipmentInfoModel();

        public object? Command { get; set; }

        public EquipmentInfoViewModel(IWritableConfig<ClientConfig> config, 
            GrpcC2DRequests grpcC2DRequests, IWindowManager windowManager)
        {
            _config = config;
            _grpcC2DRequests = grpcC2DRequests;
            _windowManager = windowManager;
        }


        public void InitializeForAdd(Guid nodeId)
        {
            _mode = ViewMode.Add;
            NodeId = nodeId;
            Model.SetSelectedRadioButton(EquipmentType.Closure);
            Model.IsRightCableReserveEnabled = true;
        }

        public void InitializeForUpdate(Equipment equipment)
        {
            _mode = ViewMode.Update;
            Equipment = equipment;
            EquipmentId = equipment.EquipmentId;
            NodeId = equipment.NodeId;

            Model.Title = equipment.Title;
            Model.SetSelectedRadioButton(equipment.Type);
            Model.CableReserveLeft = equipment.CableReserveLeft;
            Model.CableReserveRight = equipment.CableReserveRight;
            Model.Comment = equipment.Comment;

            Model.IsRightCableReserveEnabled = equipment.Type != EquipmentType.Terminal &&
                                               equipment.Type != EquipmentType.CableReserve;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _mode == ViewMode.Add ? Resources.SID_Add_Equipment : Resources.SID_Edit_Equipment;
        }

        public async void Save()
        {
            var eqType = Model.GetSelectedRadioButton();
            var maxCableReserve = _config.Value.Miscellaneous.MaxCableReserve;
            if (Model.CableReserveLeft > maxCableReserve || Model.CableReserveRight > maxCableReserve)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, string.Format(Resources.SID_Cable_reserve_could_not_be_more_than__0__m, maxCableReserve));
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            if (Model.Type == EquipmentType.Terminal && Model.CableReserveRight > 0)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"Запас кабеля после оконечного кросса не имеет смысла");
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            if (_mode == ViewMode.Update)
            {

                var cmd = new UpdateEquipment()
                {
                    EquipmentId = EquipmentId,
                    Title = Model.Title,
                    Type = eqType,
                    CableReserveLeft = Model.CableReserveLeft,
                    CableReserveRight = Model.CableReserveRight,
                    Comment = Model.Comment,
                };
                await _grpcC2DRequests.SendEventSourcingCommand(cmd);
            }

            if (_mode == ViewMode.Add)
            {
                EquipmentId = Guid.NewGuid();
                var cmd = new AddEquipmentIntoNode()
                {
                    EquipmentId = EquipmentId,
                    NodeId = NodeId,
                    Title = Model.Title,
                    Type = eqType,
                    CableReserveLeft = Model.CableReserveLeft,
                    CableReserveRight = Model.CableReserveRight,
                    Comment = Model.Comment,
                };
                Command = cmd;
                // for equipment addition this part of command 
                // would be OUTSIDE amplified with list of trace which use this equipment 
            }

            await TryCloseAsync();
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }

    }
}
