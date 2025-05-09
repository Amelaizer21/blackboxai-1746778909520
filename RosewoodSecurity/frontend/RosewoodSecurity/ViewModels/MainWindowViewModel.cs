using System;
using System.Windows.Input;
using RosewoodSecurity.Models;
using RosewoodSecurity.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RosewoodSecurity.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authService;
        private readonly IThemeService _themeService;
        private readonly IDialogService _dialogService;

        private bool _isLoggedIn;
        private bool _isAdmin;
        private bool _isDarkTheme;
        private object _currentView;
        private UserInfo _currentUser;

        public MainWindowViewModel(
            INavigationService navigationService,
            IAuthenticationService authService,
            IThemeService themeService,
            IDialogService dialogService)
        {
            _navigationService = navigationService;
            _authService = authService;
            _themeService = themeService;
            _dialogService = dialogService;

            // Initialize commands
            NavigateCommand = new RelayCommand<string>(Navigate);
            LogoutCommand = new RelayCommand(Logout);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);

            // Subscribe to authentication changes
            _authService.AuthenticationChanged += OnAuthenticationChanged;
            
            // Subscribe to navigation changes
            _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // Initialize theme
            IsDarkTheme = _themeService.IsDarkTheme;
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set => SetProperty(ref _isLoggedIn, value);
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            private set => SetProperty(ref _isAdmin, value);
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    _themeService.SetTheme(value);
                }
            }
        }

        public object CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        public UserInfo CurrentUser
        {
            get => _currentUser;
            private set => SetProperty(ref _currentUser, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        private void Navigate(string viewName)
        {
            if (!IsLoggedIn) return;

            switch (viewName)
            {
                case "Dashboard":
                    _navigationService.NavigateTo<DashboardViewModel>();
                    break;
                case "CheckInOut":
                    _navigationService.NavigateTo<CheckInOutViewModel>();
                    break;
                case "Keys":
                    if (IsAdmin)
                        _navigationService.NavigateTo<KeysManagementViewModel>();
                    break;
                case "AccessCards":
                    if (IsAdmin)
                        _navigationService.NavigateTo<AccessCardsViewModel>();
                    break;
                case "Reports":
                    if (IsAdmin)
                        _navigationService.NavigateTo<ReportsViewModel>();
                    break;
            }
        }

        private void Logout()
        {
            try
            {
                _authService.Logout();
                _navigationService.NavigateTo<LoginViewModel>();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Logout Error", 
                    "An error occurred while logging out. Please try again.");
                Console.WriteLine($"Logout error: {ex}");
            }
        }

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        private void OnAuthenticationChanged(object sender, AuthenticationEventArgs e)
        {
            IsLoggedIn = e.IsAuthenticated;
            CurrentUser = e.CurrentUser;
            IsAdmin = e.CurrentUser?.Role == "Admin";

            if (IsLoggedIn)
            {
                _navigationService.NavigateTo<DashboardViewModel>();
            }
            else
            {
                _navigationService.NavigateTo<LoginViewModel>();
            }
        }

        private void OnCurrentViewModelChanged(object sender, object viewModel)
        {
            CurrentView = viewModel;
        }

        public override void Dispose()
        {
            _authService.AuthenticationChanged -= OnAuthenticationChanged;
            _navigationService.CurrentViewModelChanged -= OnCurrentViewModelChanged;
            base.Dispose();
        }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }

    public class AuthenticationEventArgs : EventArgs
    {
        public bool IsAuthenticated { get; set; }
        public UserInfo CurrentUser { get; set; }
    }
}
