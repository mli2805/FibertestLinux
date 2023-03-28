using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.Utils;
using Fibertest.Utils.Setup;
using Fibertest.WpfCommonViews;
using Microsoft.Win32;

namespace Fibertest.WpfClient
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWritableConfig<ClientConfig> _config;
        private Rtu _rtu = null!;
        private Trace _trace = null!;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;

        private readonly CurrentGis _currentGis;
        private readonly GrpcC2RService _grpcC2RService;
        private readonly GraphGpsCalculator _graphGpsCalculator;
        private readonly BaseRefMessages _baseRefMessages;


        private readonly string _savedInDb = Resources.SID_Saved_in_DB;
        private string? _lastChosenFile;

        public string RtuTitle { get; private set; } = null!;

        public string TraceTitle { get; private set; } = null!;
        public string TracePortOnRtu { get; private set; } = null!;

        private string? _preciseBaseFilename;
        public string? PreciseBaseFilename
        {
            get => _preciseBaseFilename;
            set
            {
                if (value == _preciseBaseFilename) return;
                _preciseBaseFilename = value;
                IsButtonSaveEnabled = true;
                NotifyOfPropertyChange();
            }
        }

        private string? _fastBaseFilename;
        public string? FastBaseFilename
        {
            get => _fastBaseFilename;
            set
            {
                if (value == _fastBaseFilename) return;
                _fastBaseFilename = value;
                IsButtonSaveEnabled = true;
                NotifyOfPropertyChange();
            }
        }

        private string? _additionalBaseFilename;
        public string? AdditionalBaseFilename
        {
            get => _additionalBaseFilename;
            set
            {
                if (value == _additionalBaseFilename) return;
                _additionalBaseFilename = value;
                IsButtonSaveEnabled = true;
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

        private string _initialDirectory = null!;
        private string InitialDirectory
        {
            get => _initialDirectory;
            set
            {
                if (value == _initialDirectory)
                    return;
                _initialDirectory = value;
                _config.Update(c=>c.Miscellaneous.PathToSor = InitialDirectory);
            }
        }

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

        public BaseRefsAssignViewModel(ILifetimeScope globalScope, IWritableConfig<ClientConfig> config,
            Model readModel, IWindowManager windowManager, CurrentUser currentUser, CurrentGis currentGis, 
            GrpcC2RService grpcC2RService, GraphGpsCalculator graphGpsCalculator,
             BaseRefMessages baseRefMessages)
        {
            _globalScope = globalScope;
            _config = config;
            _readModel = readModel;
            _windowManager = windowManager;
            _currentUser = currentUser;
            _currentGis = currentGis;
            _grpcC2RService = grpcC2RService;
            _graphGpsCalculator = graphGpsCalculator;
            _baseRefMessages = baseRefMessages;
        }

        public void Initialize(Trace trace)
        {
            _trace = trace;
            TraceTitle = _trace.Title;
            TracePortOnRtu = _trace.Port > 0 ? _trace.Port.ToString() : Resources.SID_not_attached;
            PreciseBaseFilename = _trace.PreciseId == Guid.Empty ? "" : _savedInDb;
            FastBaseFilename = _trace.FastId == Guid.Empty ? "" : _savedInDb;
            AdditionalBaseFilename = _trace.AdditionalId == Guid.Empty ? "" : _savedInDb;
            IsButtonSaveEnabled = false;
            IsEditEnabled = true;
            _rtu = _readModel.Rtus.First(r => r.Id == _trace.RtuId);
            RtuTitle = _rtu.Title;


            // if InitialDirectory for OpenFileDialog does not exist:
            //   when drive in InitialDirectory exists - it's ok - will be used previous path from Windows
            //   but if even drive does not exist will be thrown exception
            var pathToSor = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory) + @"\tmp";

            InitialDirectory = _config.Value.Miscellaneous.PathToSor;
            if (!Directory.Exists(InitialDirectory))
            {
                InitialDirectory = pathToSor;
                _config.Update(c=>c.Miscellaneous.PathToSor = InitialDirectory);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Base_refs_assignment;
        }

        private bool IsFilenameChanged(string filename, Guid previousBaseRefId)
        {
            return (filename != "" && filename != _savedInDb) || (filename == "" && previousBaseRefId != Guid.Empty);
        }

        #region Buttons
        public void GetPathToPrecise()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Resources.SID_Reflectogram_files,
                InitialDirectory = InitialDirectory,
                FileName = _lastChosenFile,
            };
            if (dialog.ShowDialog() == true)
            {
                PreciseBaseFilename = dialog.FileName;
                InitialDirectory = Path.GetDirectoryName(dialog.FileName) ?? "";
                _lastChosenFile = Path.GetFileName(dialog.FileName);
            }
        }
        public void GetPathToFast()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Resources.SID_Reflectogram_files,
                InitialDirectory = InitialDirectory,
                FileName = _lastChosenFile,
            };
            if (dialog.ShowDialog() == true)
            {
                FastBaseFilename = dialog.FileName;
                InitialDirectory = Path.GetDirectoryName(dialog.FileName) ?? "";
                _lastChosenFile = Path.GetFileName(dialog.FileName);
            }
        }
        public void GetPathToAdditional()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Resources.SID_Reflectogram_files,
                InitialDirectory = InitialDirectory,
                FileName = _lastChosenFile,
            };
            if (dialog.ShowDialog() == true)
            {
                AdditionalBaseFilename = dialog.FileName;
                InitialDirectory = Path.GetDirectoryName(dialog.FileName) ?? "";
                _lastChosenFile = Path.GetFileName(dialog.FileName);
            }
        }

        public void ClearPathToPrecise() { PreciseBaseFilename = ""; }
        public void ClearPathToFast() { FastBaseFilename = ""; }
        public void ClearPathToAdditional() { AdditionalBaseFilename = ""; }

        #endregion

        public async Task Save()
        {
            IsEditEnabled = false;
            var dto = PrepareDto(_trace);
            if (dto == null) return;

            if (dto.BaseRefs.Any() && await IsValidCombination() && await IsDistanceLengthAcceptable(dto, _trace))
            {
                BaseRefAssignedDto result;
                using (_globalScope.Resolve<IWaitCursor>())
                {
                    result = await _grpcC2RService.SendAnyC2RRequest<AssignBaseRefsDto, BaseRefAssignedDto>(dto);
                    // send to Db and RTU
                }

                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    _baseRefMessages.Display(result, _trace);

                await TryCloseAsync();
            }

            IsEditEnabled = true;
        }

        private async Task<bool> IsValidCombination()
        {
            if (string.IsNullOrEmpty(PreciseBaseFilename) 
                && string.IsNullOrEmpty(FastBaseFilename) 
                && string.IsNullOrEmpty(AdditionalBaseFilename)) return true;
            if (!string.IsNullOrEmpty(PreciseBaseFilename) 
                && !string.IsNullOrEmpty(FastBaseFilename)) return true;

            if (string.IsNullOrEmpty(PreciseBaseFilename))
            {
                var vm1 = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Precise_base_ref_must_be_set_);
                await _windowManager.ShowDialogWithAssignedOwner(vm1);
                return false;
            }

            var vmp = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Fast_base_ref_must_be_set_);
            await _windowManager.ShowDialogWithAssignedOwner(vmp);
            return false;
        }

        private async Task<bool> IsDistanceLengthAcceptable(AssignBaseRefsDto dto, Trace trace)
        {
            if (_currentGis.IsWithoutMapMode) return true;

            var precise = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (precise == null || precise.Id == Guid.Empty) return true;

            var message = SorData.TryGetFromBytes(precise.SorBytes, out var otdrKnownBlocks);
            if (message != "") return true;

            var gpsDistance = $@"{_graphGpsCalculator.CalculateTraceGpsLengthKm2(trace):#,0.##}";
            var opticalLength = $@"{otdrKnownBlocks!.GetTraceLengthKm():#,0.##}";
            return await _baseRefMessages.IsLengthDifferenceAcceptable(gpsDistance, opticalLength);
        }

        public AssignBaseRefsDto? PrepareDto(Trace trace)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return null;
            var dto = new AssignBaseRefsDto(trace.RtuId, rtu.RtuMaker, 
                trace.TraceId,new List<BaseRefDto>(), new List<int>())
            {
                OtdrId = rtu.OtdrId ?? "",
                OtauPortDto = trace.OtauPort,
            };

            if (trace.OtauPort != null && !trace.OtauPort.IsPortOnMainCharon && rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto(trace.OtauPort.MainCharonPort, true)
                {
                    OtauId = rtu.MainVeexOtau.id,
                };
            }

            var baseRefs = new List<BaseRefDto>();
            if (IsFilenameChanged(PreciseBaseFilename!, trace.PreciseId))
            {
                var baseRefDto = BaseRefDtoFactory
                    .CreateFromFile(PreciseBaseFilename!, BaseRefType.Precise, _currentUser.UserName);
                if (trace.PreciseId != Guid.Empty)
                    dto.DeleteOldSorFileIds.Add(_readModel.BaseRefs.First(b => b.Id == trace.PreciseId).SorFileId);
                baseRefs.Add(baseRefDto);
            }

            if (IsFilenameChanged(FastBaseFilename!, trace.FastId))
            {
                var baseRefDto = BaseRefDtoFactory
                    .CreateFromFile(FastBaseFilename!, BaseRefType.Fast, _currentUser.UserName);
                if (trace.FastId != Guid.Empty)
                    dto.DeleteOldSorFileIds.Add(_readModel.BaseRefs.First(b => b.Id == trace.FastId).SorFileId);
                baseRefs.Add(baseRefDto);
            }

            if (IsFilenameChanged(AdditionalBaseFilename!, trace.AdditionalId))
            {
                var baseRefDto = BaseRefDtoFactory
                    .CreateFromFile(AdditionalBaseFilename!, BaseRefType.Additional, _currentUser.UserName);
                if (trace.AdditionalId != Guid.Empty)
                    dto.DeleteOldSorFileIds.Add(_readModel.BaseRefs.First(b => b.Id == trace.AdditionalId).SorFileId);
                baseRefs.Add(baseRefDto);
            }

            dto.BaseRefs = baseRefs;
            return dto;
        }

        public async void Cancel()
        {
            await TryCloseAsync();
        }
    }
}
