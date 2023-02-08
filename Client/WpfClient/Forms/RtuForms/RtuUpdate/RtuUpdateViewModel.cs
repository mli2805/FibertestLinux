using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using AutoMapper;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.WpfCommonViews;
using Fibertest.StringResources;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class RtuUpdateViewModel : Screen, IDataErrorInfo
    {
        public Guid RtuId;
        private Rtu _originalRtu;
        private Node _originalNode;
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly IWindowManager _windowManager;
        private bool _isInCreationMode;

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        private string _comment;
        public string Comment
        {
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonSaveEnabled;

        public bool IsButtonSaveEnabled
        {
            get => _isButtonSaveEnabled;
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }
        public GpsInputViewModel GpsInputViewModel { get; set; }
        public bool HasPrivilegies { get; set; }
        public Visibility GisVisibility { get; set; }

        private bool _isEditEnabled;
        public bool IsEditEnabled
        {
            get => _isEditEnabled;
            set
            {
                if (value == _isEditEnabled) return;
                _isEditEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public TceRelationInfo TceRelationInfo { get; set; } = new TceRelationInfo();

        public RtuUpdateViewModel(ILifetimeScope globalScope, CurrentUser currentUser, CurrentGis currentGis,
            Model readModel, GraphReadModel graphReadModel,
            GrpcC2DService grpcC2DService, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _grpcC2DService = grpcC2DService;
            _windowManager = windowManager;
            IsEditEnabled = true;
            HasPrivilegies = currentUser.Role <= Role.Root;
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Initialize(Guid rtuId)
        {
            RtuId = rtuId;
            _originalRtu = _readModel.Rtus.First(r => r.Id == RtuId);

            _originalNode = _readModel.Nodes.First(n => n.NodeId == _originalRtu.NodeId);
            GpsInputViewModel = _globalScope.Resolve<GpsInputViewModel>();
            GpsInputViewModel.Initialize(_originalNode, HasPrivilegies);

            Title = _originalRtu.Title;
            Comment = _originalRtu.Comment;

            var tceIds = _readModel.GponPortRelations.Where(r => r.RtuId == rtuId).Select(l => l.TceId).ToList();
            TceRelationInfo.Tces = _readModel.TcesNew.Where(t => tceIds.Contains(t.Id)).Select(e => e).ToList();
            if (TceRelationInfo.Tces.Any())
                TceRelationInfo.Visibility = Visibility.Visible;
        }

        public void Initialize(RequestAddRtuAtGpsLocation request)
        {
            _isInCreationMode = true;
            var nodeId = Guid.NewGuid();
            _originalNode = new Node() { NodeId = nodeId, Position = new PointLatLng(request.Latitude, request.Longitude) };
            RtuId = Guid.NewGuid();
            _originalRtu = new Rtu() { Id = RtuId, NodeId = nodeId };

            GpsInputViewModel = _globalScope.Resolve<GpsInputViewModel>();
            GpsInputViewModel.Initialize(_originalNode, HasPrivilegies);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_RTU_Information;
        }

        public async void Save()
        {
            IsEditEnabled = false;
            var result = _isInCreationMode
                ? await CreateRtu()
                : await UpdateRtu();
            IsEditEnabled = true;
            if (result) await TryCloseAsync();
        }

        private async Task<bool> CreateRtu()
        {
            var errorMessage = GpsInputViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            var cmd = new AddRtuAtGpsLocation(_originalRtu.Id, _originalNode.NodeId, position.Lat, position.Lng, Title)
            {
                Comment = Comment,
            };

            RequestAnswer result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                result = await _grpcC2DService.SendEventSourcingCommand(cmd);
            }

            if (result.ReturnCode != ReturnCode.Ok)
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, @"CreateRtu: " + result);
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return false;
            }

            return true;
        }

        private async Task<bool> UpdateRtu()
        {
            IMapper mapper =
                new MapperConfiguration(cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();
            UpdateRtu cmd = mapper.Map<UpdateRtu>(this);
            var errorMessage = GpsInputViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            cmd.Position = position;
            var result = await _grpcC2DService.SendEventSourcingCommand(cmd);
            if (result.ReturnCode != ReturnCode.Ok)
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, result.ErrorMessage!);
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return false;
            }
            return true;
        }

        public async void Cancel()
        {
            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _originalNode.NodeId);
            if (nodeVm != null)
            {
                nodeVm.Position = _originalNode.Position;
            }

            await TryCloseAsync();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (string.IsNullOrEmpty(Title))
                            errorMessage = Resources.SID_Title_is_required;
                        if (_readModel.Rtus.Any(n => n.Title == Title && n.Id != _originalRtu.Id))
                            errorMessage = Resources.SID_There_is_a_rtu_with_the_same_title;
                        IsButtonSaveEnabled = HasPrivilegies && errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }


}
