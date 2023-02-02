using System;
using System.ComponentModel;
using AutoMapper;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class ZoneViewModel : Screen, IDataErrorInfo
    {
        private readonly GrpcC2DRequests _grpcC2DRequests;

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();

        private bool _isInCreationMode;

        public Guid ZoneId { get; set; }

        private string? _title;
        public string? Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        private string? _comment;
        public string? Comment
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

        public ZoneViewModel(GrpcC2DRequests grpcC2DRequests)
        {
            _grpcC2DRequests = grpcC2DRequests;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInCreationMode ? Resources.SID_New_responsibility_zone_creation : Resources.SID_Update_responsibility_zone;
        }

        public void Initialize(Zone selectedZone)
        {
            _isInCreationMode = false;
            ZoneId = selectedZone.ZoneId;
            Title = selectedZone.Title;
            Comment = selectedZone.Comment;
        }

        public void Initialize()
        {
            _isInCreationMode = true;
            ZoneId = Guid.NewGuid();
        }

        public async void Save()
        {
            object cmd;
            if (_isInCreationMode)
                cmd = Mapper.Map<AddZone>(this);
            else
                cmd = Mapper.Map<UpdateZone>(this);
            await _grpcC2DRequests.SendEventSourcingCommand(cmd); 
            await TryCloseAsync();
        }

        public async void Cancel()
        {
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
                        if (string.IsNullOrEmpty(_title?.Trim()))
                            errorMessage = Resources.SID_Title_is_required;
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; } = string.Empty;
    }
}
