using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.OtdrDataFormat;
using Fibertest.WpfCommonViews;
using GMap.NET;

namespace Fibertest.WpfClient
{
    public class OneLandmarkViewModel : PropertyChangedBase
    {
        public string TraceTitle;
        public DateTime PreciseTimestamp;
        public int SorFileId;
        public Guid RtuId;

        private readonly CurrentGis _currentGis;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TabulatorViewModel _tabulatorViewModel;

        public bool IsIncludeEquipmentEnabled
        {
            get => _isIncludeEquipmentEnabled;
            set
            {
                if (value == _isIncludeEquipmentEnabled) return;
                _isIncludeEquipmentEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsExcludeEquipmentEnabled
        {
            get => _isExcludeEquipmentEnabled;
            set
            {
                if (value == _isExcludeEquipmentEnabled) return;
                _isExcludeEquipmentEnabled = value;
                NotifyOfPropertyChange();
            }
        }


        private GpsInputSmallViewModel _gpsInputSmallViewModel;
        public GpsInputSmallViewModel GpsInputSmallViewModel
        {
            get => _gpsInputSmallViewModel;
            set
            {
                if (Equals(value, _gpsInputSmallViewModel)) return;
                _gpsInputSmallViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private Graph.Landmark _landmarkBeforeChanges;

        private EquipmentTypeComboItem _selectedEquipmentTypeItem;
        public EquipmentTypeComboItem SelectedEquipmentTypeItem
        {
            get => _selectedEquipmentTypeItem;
            set
            {
                if (Equals(value, _selectedEquipmentTypeItem) || value == null) return;
                _selectedEquipmentTypeItem = value;
                SelectedLandmark.EquipmentType = value.Type;
                NotifyOfPropertyChange();
            }
        }

        private Graph.Landmark _selectedLandmark;
        public Graph.Landmark SelectedLandmark
        {
            get => _selectedLandmark;
            set
            {
                if (value == null) return;
                _selectedLandmark = value;
                InitializeUserControl();
                NotifyOfPropertyChange();
            }
        }

        private void InitializeUserControl()
        {
            _landmarkBeforeChanges = (Graph.Landmark)_selectedLandmark.Clone();
            GpsInputSmallViewModel.Initialize(SelectedLandmark.GpsCoors);
            ComboItems = GetItems(SelectedLandmark.EquipmentType);
            SelectedEquipmentTypeItem = ComboItems.First(i => i.Type == SelectedLandmark.EquipmentType);
            IsEquipmentEnabled = HasPrivileges && SelectedLandmark.EquipmentType != EquipmentType.EmptyNode &&
                                 SelectedLandmark.EquipmentType != EquipmentType.Rtu;
        }

        private List<EquipmentTypeComboItem> _comboItems;
        public List<EquipmentTypeComboItem> ComboItems
        {
            get => _comboItems;
            set
            {
                if (Equals(value, _comboItems)) return;
                _comboItems = value;
                NotifyOfPropertyChange();
            }
        }

        private List<EquipmentTypeComboItem> GetItems(EquipmentType type)
        {
            if (type == EquipmentType.Rtu) return new List<EquipmentTypeComboItem> { new EquipmentTypeComboItem(EquipmentType.Rtu) };
            if (type == EquipmentType.EmptyNode) return new List<EquipmentTypeComboItem> { new EquipmentTypeComboItem(EquipmentType.EmptyNode) };
            return new List<EquipmentTypeComboItem>
            {
                new EquipmentTypeComboItem(EquipmentType.Closure),
                new EquipmentTypeComboItem(EquipmentType.Cross),
                new EquipmentTypeComboItem(EquipmentType.Terminal),
                new EquipmentTypeComboItem(EquipmentType.CableReserve),
                new EquipmentTypeComboItem(EquipmentType.Other)
            };
        }

        public bool HasPrivileges { get; set; }

        private bool _isEquipmentEnabled;
        private bool _isIncludeEquipmentEnabled;
        private bool _isExcludeEquipmentEnabled;
        private bool _isFromBaseRef;

        public bool IsEquipmentEnabled
        {
            get => _isEquipmentEnabled;
            set
            {
                if (value == _isEquipmentEnabled) return;
                _isEquipmentEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFromBaseRef
        {
            get => _isFromBaseRef;
            set
            {
                if (value == _isFromBaseRef) return;
                _isFromBaseRef = value;
                NotifyOfPropertyChange();
            }
        }

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

        public OneLandmarkViewModel(CurrentUser currentUser, CurrentGis currentGis,
            GpsInputSmallViewModel gpsInputSmallViewModel, IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager,
            GraphReadModel graphReadModel, Model readModel,
            ReflectogramManager reflectogramManager, TabulatorViewModel tabulatorViewModel)
        {
            HasPrivileges = currentUser.Role <= Role.Root;
            IsEditEnabled = true;
            _currentGis = currentGis;
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _tabulatorViewModel = tabulatorViewModel;
            GpsInputSmallViewModel = gpsInputSmallViewModel;
        }

        public async void Apply()
        {
            IsEditEnabled = false;
            _graphReadModel.ExtinguishAllNodes();
            var unused = await ApplyingProcess();
            IsEditEnabled = true;
        }

        private async Task<bool> ApplyingProcess()
        {
            var result = await ApplyEquipment();
            if (result != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"ApplyEquipment: " + result);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            result = await ApplyNode();
            if (result != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"ApplyNode: " + result);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            return true;
        }

        private async Task<string> ApplyEquipment()
        {
            if (_landmarkBeforeChanges.EquipmentTitle != SelectedLandmark.EquipmentTitle ||
                _landmarkBeforeChanges.EquipmentType != SelectedLandmark.EquipmentType)
            {
                var equipment = _readModel.Equipments.First(e => e.EquipmentId == SelectedLandmark.EquipmentId);
                return await _c2DWcfManager.SendCommandAsObj(
                    new UpdateEquipment
                    {
                        EquipmentId = SelectedLandmark.EquipmentId,
                        Title = SelectedLandmark.EquipmentTitle,
                        Type = SelectedLandmark.EquipmentType,
                        CableReserveLeft = equipment.CableReserveLeft,
                        CableReserveRight = equipment.CableReserveRight,
                        Comment = equipment.Comment,
                    });
            }
            return null;
        }

        private async Task<string> ApplyNode()
        {
            var errorMessage = GpsInputSmallViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return null;
            }

            if (_landmarkBeforeChanges.NodeTitle != SelectedLandmark.NodeTitle ||
                _landmarkBeforeChanges.NodeComment != SelectedLandmark.NodeComment ||
                _landmarkBeforeChanges.GpsCoors != position)
            {
                var cmd = SelectedLandmark.EquipmentType == EquipmentType.Rtu
                    ? (object)new UpdateRtu()
                    {
                        RtuId = RtuId,
                        Title = SelectedLandmark.NodeTitle,
                        Comment = SelectedLandmark.NodeComment,
                        Position = position,
                    }
                    : new UpdateAndMoveNode
                    {
                        NodeId = SelectedLandmark.NodeId,
                        Title = SelectedLandmark.NodeTitle,
                        Comment = SelectedLandmark.NodeComment,
                        Position = position,
                    };
                return await _c2DWcfManager.SendCommandAsObj(cmd);
            }
            return null;
        }

        public async void CancelChanges()
        {
            await Cancel(@"CancelChanges");
        }

        private const string RevertChanges = "CancelChanges";
        public const string BeforeNew = "BeforeNew";
        public const string CanClose = "CanClose";

        public async Task Cancel(string reason)
        {
            if (_landmarkBeforeChanges == null) return;

            SelectedLandmark = _landmarkBeforeChanges;
            if (reason != BeforeNew)
            {
                var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == SelectedLandmark.NodeId);
                if (nodeVm != null)
                    nodeVm.IsHighlighted = false;
            }

            var errorMessage = GpsInputSmallViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            if (_showOnMapPressed)
            {
                _showOnMapPressed = false;
                var node = _readModel.Nodes.First(n => n.NodeId == SelectedLandmark.NodeId);
                if (node.Position != position)
                {
                    node.Position = position;
                    _graphReadModel.MainMap.SetPositionWithoutFiringEvent(node.Position);
                    var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == SelectedLandmark.NodeId);
                    if (nodeVm != null)
                        nodeVm.Position = position;
                    await _graphReadModel.RefreshVisiblePart();
                    if (nodeVm != null)
                        nodeVm.IsHighlighted = reason == RevertChanges;
                }
            }
        }

        private bool _showOnMapPressed;

        public async void ShowLandmarkOnMap()
        {
            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;
            _showOnMapPressed = true;

            await Task.Delay(100);

            if (_currentGis.ThresholdZoom > _graphReadModel.MainMap.Zoom)
                _graphReadModel.MainMap.Zoom = _currentGis.ThresholdZoom;
            _graphReadModel.ExtinguishAllNodes();

            var node = _readModel.Nodes.First(n => n.NodeId == SelectedLandmark.NodeId);
            var errorMessage = GpsInputSmallViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            node.Position = position; 
            node.IsHighlighted = true;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(node.Position);
            await _graphReadModel.RefreshVisiblePart();

            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == SelectedLandmark.NodeId);
            nodeVm.Position = position;
            nodeVm.IsHighlighted = true;
        }

        public void ShowReflectogram()
        {
            _reflectogramManager.SetTempFileName(TraceTitle, BaseRefType.Precise.ToString(), PreciseTimestamp);
            _reflectogramManager.ShowBaseReflectogramWithSelectedLandmark(SorFileId, SelectedLandmark.Number + 1);
        }
    }
}
