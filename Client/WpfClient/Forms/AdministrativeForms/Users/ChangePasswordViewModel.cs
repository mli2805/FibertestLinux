using System.ComponentModel;
using System.Windows;
using AutoMapper;
using Caliburn.Micro;
using Fibertest.Graph;
using Fibertest.StringResources;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class ChangePasswordViewModel : Screen
    {
        private readonly GrpcC2DRequests _grpcC2DRequests;
        private User _user = null!;

        public PasswordViewModel OldPasswordVm { get; set; } = new PasswordViewModel();

        private bool _isChangePasswordEnabled;
        public bool IsChangePasswordEnabled
        {
            get => _isChangePasswordEnabled;
            set
            {
                if (value == _isChangePasswordEnabled) return;
                _isChangePasswordEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public PasswordViewModel NewPasswordVm1 { get; set; } = new PasswordViewModel();
        public PasswordViewModel NewPasswordVm2 { get; set; } = new PasswordViewModel();

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

        private Visibility _newPasswordBlockVisibility = Visibility.Collapsed;
        public Visibility NewPasswordBlockVisibility
        {
            get => _newPasswordBlockVisibility;
            set
            {
                if (value == _newPasswordBlockVisibility) return;
                _newPasswordBlockVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isNewPasswordFocused;
        public bool IsNewPasswordFocused
        {
            get => _isNewPasswordFocused;
            set
            {
                if (value == _isNewPasswordFocused) return;
                _isNewPasswordFocused = value;
                NotifyOfPropertyChange();
            }
        }

        private string _explanation = "";

        public string Explanation
        {
            get => _explanation;
            set
            {
                if (value == _explanation) return;
                _explanation = value;
                NotifyOfPropertyChange();
            }
        }

        public ChangePasswordViewModel(GrpcC2DRequests grpcC2DRequests)
        {
            _grpcC2DRequests = grpcC2DRequests;
        }

        public void Initialize(User user)
        {
            _user = user;
            OldPasswordVm.Label = "";
            NewPasswordVm1.Label = "";
            NewPasswordVm2.Label = "";
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Change_password;
            NewPasswordVm1.PropertyChanged += NewPasswordVm1_PropertyChanged;
            NewPasswordVm2.PropertyChanged += NewPasswordVm2_PropertyChanged;
        }

        private void NewPasswordVm2_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            IsButtonSaveEnabled = IsPasswordValid();
        }

        private void NewPasswordVm1_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            IsButtonSaveEnabled = IsPasswordValid();
        }

        public void CompareWithCurrent()
        {
            IsChangePasswordEnabled = _user.EncodedPassword == OldPasswordVm.Password.GetHashString();
            Explanation = IsChangePasswordEnabled ? "" : Resources.SID_Wrong_password;
            NewPasswordBlockVisibility = IsChangePasswordEnabled ? Visibility.Visible : Visibility.Collapsed;
            IsNewPasswordFocused = IsChangePasswordEnabled;
            IsButtonSaveEnabled = false;
        }

        public async void Save()
        {
            IMapper mapper = new MapperConfiguration(
                cfg => cfg.AddProfile<MappingModelToCmdProfile>()).CreateMapper();
            var cmd = mapper.Map<UpdateUser>(_user);
            cmd.EncodedPassword = NewPasswordVm1.Password.GetHashString();
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
                    case "Password1":
                    case "Password2":
                        if (string.IsNullOrEmpty(NewPasswordVm1.Password.Trim())
                            || string.IsNullOrEmpty(NewPasswordVm2.Password.Trim()))
                            errorMessage = Resources.SID_Password_should_be_set;
                        else if (NewPasswordVm1.Password != NewPasswordVm2.Password)
                            errorMessage = Resources.SID_Passwords_don_t_match;
                        break;
                }
                IsButtonSaveEnabled = errorMessage == string.Empty;
                return errorMessage;
            }
        }

        private bool IsPasswordValid()
        {
            if (string.IsNullOrEmpty(NewPasswordVm1.Password.Trim())) return false;
            return NewPasswordVm1.Password == NewPasswordVm2.Password;
        }

    }
}
