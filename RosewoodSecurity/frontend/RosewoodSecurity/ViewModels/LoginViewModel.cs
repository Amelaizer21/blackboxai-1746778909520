using System;
using System.Threading.Tasks;
using System.Windows.Input;
using RosewoodSecurity.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RosewoodSecurity.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        
        private string _username;
        private string _errorMessage;
        private string _twoFactorCode;
        private bool _showTwoFactorInput;
        private bool _isBusy;

        public LoginViewModel(
            IAuthenticationService authService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            LoginCommand = new RelayCommand(async () => await LoginAsync(), () => !IsBusy);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string TwoFactorCode
        {
            get => _twoFactorCode;
            set => SetProperty(ref _twoFactorCode, value);
        }

        public bool ShowTwoFactorInput
        {
            get => _showTwoFactorInput;
            set => SetProperty(ref _showTwoFactorInput, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;

                // Get password from PasswordBox in code-behind
                var password = GetPassword();

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage = "Please enter both username and password.";
                    return;
                }

                // First phase authentication
                if (!ShowTwoFactorInput)
                {
                    var requiresTwoFactor = await _authService.InitiateLoginAsync(Username, password);
                    if (requiresTwoFactor)
                    {
                        ShowTwoFactorInput = true;
                        return;
                    }
                }
                // Second phase (2FA) authentication
                else if (!string.IsNullOrWhiteSpace(TwoFactorCode))
                {
                    var success = await _authService.ValidateTwoFactorAsync(TwoFactorCode);
                    if (!success)
                    {
                        ErrorMessage = "Invalid 2FA code. Please try again.";
                        return;
                    }
                }

                // Navigate to dashboard on successful login
                _navigationService.NavigateTo("Dashboard");
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "Invalid username or password.";
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Login Error", 
                    "An error occurred while trying to log in. Please try again.");
                // Log the exception details
                Console.WriteLine($"Login error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // This will be called from code-behind to set the password
        internal Action<string> GetPassword { get; set; }
    }

    // Simple implementation of ICommand for the example
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged();
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
