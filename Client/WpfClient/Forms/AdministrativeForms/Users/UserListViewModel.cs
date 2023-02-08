using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public class UserListViewModel : Screen
    {
        private List<User> _users = null!;
        private List<Zone> _zones = null!;
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly EventArrivalNotifier _eventArrivalNotifier;
        private readonly IWindowManager _windowManager;
        private readonly GrpcC2DService _grpcC2DService;
        private readonly CurrentUser _currentUser;

        private ObservableCollection<UserVm> _rows = new ObservableCollection<UserVm>();
        public ObservableCollection<UserVm> Rows
        {
            get => _rows;
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private UserVm? _selectedUser;
        public UserVm? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                NotifyOfPropertyChange(nameof(CanEdit));
                NotifyOfPropertyChange(nameof(CanRemove));
            }
        }

        public static List<Role> Roles { get; set; } = null!;
        public bool CanAdd => _currentUser.Role <= Role.Root;
        public bool CanEdit => _currentUser.Role <= Role.Root || _currentUser.UserId == SelectedUser?.UserId;
        public bool CanRemove => _currentUser.Role <= Role.Root 
                                 && SelectedUser?.Role != Role.Root && SelectedUser?.Role != Role.SecurityAdmin;

        public UserListViewModel(ILifetimeScope globalScope, Model readModel, 
            EventArrivalNotifier eventArrivalNotifier, IWindowManager windowManager, 
            GrpcC2DService grpcC2DService, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _eventArrivalNotifier = eventArrivalNotifier;
            _windowManager = windowManager;
            _grpcC2DService = grpcC2DService;
            _currentUser = currentUser;

            Initialize();
        }

        private void Initialize()
        {
            _users = _readModel.Users;
            _zones = _readModel.Zones;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            foreach (var user in _users.Where(u => u.Role >= _currentUser.Role))
                Rows.Add(new UserVm(user, _zones.First(z => z.ZoneId == user.ZoneId).Title));

            _eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
            SelectedUser = Rows.First();
        }

        private void _eventArrivalNotifier_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Rows = new ObservableCollection<UserVm>();
            foreach (var user in _users.Where(u => u.Role >= _currentUser.Role))
                Rows.Add(new UserVm(user, _zones.First(z => z.ZoneId == user.ZoneId).Title));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_list;
        }

        #region One User Actions
        public async void AddNewUser()
        {
            var vm = _globalScope.Resolve<UserViewModel>();
            vm.InitializeForCreate();
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void ChangeUser()
        {
            if (SelectedUser!.Role != Role.SecurityAdmin)
            {
                var userInWork = (UserVm)SelectedUser.Clone();
                var vm = _globalScope.Resolve<UserViewModel>();
                vm.InitializeForUpdate(userInWork);
                await _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            else
                // ChangeSecurityAdminPassword();
                ChangePassword();
        }

        // private void ChangeSecurityAdminPassword()
        // {
        //     _securityAdminConfirmationViewModel.Initialize();
        //     _windowManager.ShowDialog(_securityAdminConfirmationViewModel);
        //     if (!_securityAdminConfirmationViewModel.IsOkPressed)
        //         return;
        //
        //     var admin = _readModel.Users.First(u => u.Role == Role.SecurityAdmin);
        //     if (_securityAdminConfirmationViewModel.PasswordViewModel.Password.GetHashString() != admin.EncodedPassword)
        //     {
        //         var strs = new List<string>() { Resources.SID_Wrong_password };
        //         var mb = new MyMessageBoxViewModel(MessageType.Information, strs, 0);
        //         _windowManager.ShowDialogWithAssignedOwner(mb);
        //         return;
        //     }
        //     // if confirmed - open special form
        // }

        private async void ChangePassword()
        {
            var vm = _globalScope.Resolve<ChangePasswordViewModel>();
            var admin = _readModel.Users.First(u => u.Role == Role.SecurityAdmin);
            vm.Initialize(admin);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void RemoveUser()
        {
            var cmd = new RemoveUser() { UserId = SelectedUser!.UserId };
            await _grpcC2DService.SendEventSourcingCommand(cmd); 
        }
        #endregion

        public async void Close()
        {
            await TryCloseAsync();
        }
    }
}
