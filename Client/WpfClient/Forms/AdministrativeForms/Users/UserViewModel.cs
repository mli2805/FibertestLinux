using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class UserViewModel : Screen, IDataErrorInfo
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly GrpcC2DService _grpcC2DService;
        private bool _isInCreationMode;

        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        public UserVm UserInWork { get; set; } = null!;

        public bool IsntItRoot { get; set; }

        private string? _password1;
        public string? Password1
        {
            get => _password1;
            set
            {
                if (value == _password1) return;
                _password1 = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Password2));
            }
        }

        private string? _password2;
        public string? Password2
        {
            get => _password2;
            set
            {
                if (value == _password2) return;
                _password2 = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Password1));
            }
        }

        public bool IsPasswordsEnabled { get; set; }


        public List<Role> Roles { get; set; } = null!;
        public List<Zone> Zones { get; set; } = null!;

        private Zone? _selectedZone;
        public Zone? SelectedZone
        {
            get => _selectedZone;
            set
            {
                if (Equals(value, _selectedZone)) return;
                _selectedZone = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsEditEnabled { get; set; }

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

        public Visibility ChangePasswordVisibility { get; set; }

        public UserViewModel(ILifetimeScope globalScope, IWindowManager windowManager, GrpcC2DService grpcC2DService,
            Model readModel, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _grpcC2DService = grpcC2DService;
            _readModel = readModel;
            _currentUser = currentUser;

            IsEditEnabled = currentUser.Role <= Role.Root;
        }

        public void InitializeForCreate()
        {
            _isInCreationMode = true;
            UserInWork = new UserVm();
            UserInWork.SmsReceiverVm.OnTestButtonPressed += SmsReceiverVm_TestButtonPressed;

            Roles = GetAvailableRoles();
            IsntItRoot = true;

            IsPasswordsEnabled = true;
            ChangePasswordVisibility = Visibility.Collapsed;

            Zones = _readModel.Zones;
            SelectedZone = Zones.First();
        }

        private async void SmsReceiverVm_TestButtonPressed(object? sender, EventArgs e)
        {
            bool res;
            using (new WaitCursor())
            {
                var dto = new SendTestNotificationDto()
                    { NotificationType = NotificationType.Sms, PhoneNumber = UserInWork.SmsReceiverVm.PhoneNumber };
                var result = await _grpcC2DService.SendAnyC2DRequest<SendTestNotificationDto, RequestAnswer>(dto);
                res = result.ReturnCode == ReturnCode.Ok;
            }

            var header = res ? MessageType.Information : MessageType.Error;
            var message = res ? Resources.SID_Test_SMS_is_placed_into_sending_queue : Resources.SID_Failed_to_place_test_SMS_into_sending_queue;
            var vm = new MyMessageBoxViewModel(header, message);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void SendTestEmail()
        {
            bool res;
            using (new WaitCursor())
            {
                var dto = new SendTestNotificationDto()
                    { NotificationType = NotificationType.Email, Email = UserInWork.EmailAddress! };
                var result = await _grpcC2DService.SendAnyC2DRequest<SendTestNotificationDto, RequestAnswer>(dto);
                res = result.ReturnCode == ReturnCode.Ok;
            }

            var header = res ? MessageType.Information : MessageType.Error;
            var message = res ? Resources.SID_Test_e_mail_sent_successfully_ : Resources.SID_Failed_to_send_test_e_mail_;
            var vm = new MyMessageBoxViewModel(header, message);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void InitializeForUpdate(UserVm user)
        {
            _isInCreationMode = false;

            UserInWork = user;
            UserInWork.SmsReceiverVm.OnTestButtonPressed += SmsReceiverVm_TestButtonPressed;

            Roles = GetAvailableRoles();
            IsntItRoot = UserInWork.Role > Role.Root;

            if (UserInWork.Role == 0)
                UserInWork.Role = Roles.First();

            Password1 = Password2 = user.EncodedPassword;
            IsPasswordsEnabled = _currentUser.Role <= Role.Root;
            ChangePasswordVisibility = _currentUser.Role <= Role.Root ? Visibility.Collapsed : Visibility.Visible;

            Zones = _readModel.Zones;
            SelectedZone = Zones.First(z => z.ZoneId == user.ZoneId);
        }

        private List<Role> GetAvailableRoles()
        {
            var skip = _currentUser.Role == Role.Developer ? 1 : 2;
            if (UserInWork.Role > Role.Root) skip = 3;
            var availableRoles = Enum.GetValues(typeof(Role)).Cast<Role>().Skip(skip).ToList();
            availableRoles.Remove(Role.SecurityAdmin);
            return availableRoles;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInCreationMode ? Resources.SID_New_user_creation : Resources.SID_Update_user_info;
        }

        public async void ChangePassword()
        {
            var vm = _globalScope.Resolve<ChangePasswordViewModel>();
            var user = _readModel.Users.First(u => u.UserId == _currentUser.UserId);
            vm.Initialize(user);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void Save()
        {
            object cmd;
            if (_isInCreationMode)
                cmd = new AddUser()
                {
                    UserId = Guid.NewGuid(),
                    Title = UserInWork.Title,
                    Role = UserInWork.Role,
                    Email = new EmailReceiver() { Address = UserInWork.EmailAddress!, IsActivated = UserInWork.IsEmailActivated },
                    Sms = UserInWork.SmsReceiverVm.Get(),
                    EncodedPassword = Password1.GetHashString(),
                    ZoneId = SelectedZone!.ZoneId,
                };
            else
                cmd = new UpdateUser()
                {
                    UserId = UserInWork.UserId,
                    Title = UserInWork.Title,
                    Role = UserInWork.Role,
                    Email = new EmailReceiver() { Address = UserInWork.EmailAddress!, IsActivated = UserInWork.IsEmailActivated },
                    Sms = UserInWork.SmsReceiverVm.Get(),
                    EncodedPassword = UserInWork.EncodedPassword == Password1 // root has right to change passwords
                        ? UserInWork.EncodedPassword 
                        : Password1.GetHashString(), 
                    ZoneId = SelectedZone!.ZoneId,
                };


            await _grpcC2DService.SendEventSourcingCommand(cmd); 
            await TryCloseAsync(true);
        }

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (string.IsNullOrEmpty(UserInWork.Title?.Trim()))
                            errorMessage = Resources.SID_Title_should_be_set_;
                        break;
                    case "Password1":
                    case "Password2":
                        if (string.IsNullOrEmpty(Password1?.Trim()) || string.IsNullOrEmpty(Password2?.Trim()))
                            errorMessage = Resources.SID_Password_should_be_set;
                        else if (Password1 != Password2)
                            errorMessage = Resources.SID_Passwords_don_t_match;
                        break;
                }
                IsButtonSaveEnabled = _currentUser.Role <= Role.Root && errorMessage == string.Empty;
                return errorMessage;

            }
        }

        public string Error { get; set; } = "";
    }
}
