using System;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isPasswordVisible;

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateLoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand OpenSubscriptionCommand { get; }

        public event Action? RequestClose;
        public event Action? RequestOpenLogin;
        public event Action? RequestOpenSubscription;

        public RegisterViewModel()
        {
            _authService = new AuthService();
            RegisterCommand = new RelayCommand(ExecuteRegister);
            NavigateLoginCommand = new RelayCommand(ExecuteNavigateLogin);
            TogglePasswordVisibilityCommand = new RelayCommand(() => IsPasswordVisible = !IsPasswordVisible);
            OpenSubscriptionCommand = new RelayCommand(() => RequestOpenSubscription?.Invoke());
        }

        private void ExecuteRegister()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "All fields are required.";
                return;
            }

            if (FullName.Trim().Length < 2)
            {
                ErrorMessage = "Full Name must be at least 2 characters.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            // All registrations create a standard account. Office Finance is unlocked via subscription.
            const string accountType = "Personal";

            if (_authService.Register(FullName, Email, Password, accountType, out string message))
            {
                // Auto login on successful registration & remember session
                _authService.Login(Email, Password, true, out _);
                ExecuteNavigateLogin();
            }
            else
            {
                ErrorMessage = message;
            }
        }

        private void ExecuteNavigateLogin()
        {
            RequestOpenLogin?.Invoke();
            RequestClose?.Invoke();
        }
    }
}
