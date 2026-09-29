using System;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly AuthService _authService;

        private int _totalTransactions;
        private decimal _lifetimeIncome;
        private decimal _lifetimeExpenses;
        private DateTime _memberSince = DateTime.UtcNow;

        // Change Password State
        private bool _isChangePasswordExpanded;
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;
        private string _passwordMessage = string.Empty;
        private bool _isPasswordError;

        public string FullName => AuthService.CurrentUser?.FullName ?? "Unknown User";
        public string Email => AuthService.CurrentUser?.Email ?? "No Email Registered";
        public string AccountType => AuthService.CurrentUser?.AccountType ?? "Personal";
        public string UserInitial => !string.IsNullOrEmpty(FullName) ? FullName.Substring(0, 1).ToUpper() : "U";

        public int TotalTransactions
        {
            get => _totalTransactions;
            set => SetProperty(ref _totalTransactions, value);
        }

        public decimal LifetimeIncome
        {
            get => _lifetimeIncome;
            set => SetProperty(ref _lifetimeIncome, value);
        }

        public decimal LifetimeExpenses
        {
            get => _lifetimeExpenses;
            set => SetProperty(ref _lifetimeExpenses, value);
        }

        public DateTime MemberSince
        {
            get => _memberSince;
            set => SetProperty(ref _memberSince, value);
        }

        public string MemberSinceText => MemberSince.ToString("MMM yyyy");

        public bool IsChangePasswordExpanded
        {
            get => _isChangePasswordExpanded;
            set => SetProperty(ref _isChangePasswordExpanded, value);
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetProperty(ref _currentPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set => SetProperty(ref _confirmNewPassword, value);
        }

        public string PasswordMessage
        {
            get => _passwordMessage;
            set => SetProperty(ref _passwordMessage, value);
        }

        public bool IsPasswordError
        {
            get => _isPasswordError;
            set => SetProperty(ref _isPasswordError, value);
        }

        public ICommand LogoutCommand { get; }
        public ICommand ToggleChangePasswordCommand { get; }
        public ICommand SavePasswordCommand { get; }

        public ProfileViewModel(MainViewModel mainViewModel, AuthService authService)
        {
            _mainViewModel = mainViewModel;
            _authService = authService;

            LogoutCommand = new RelayCommand(ExecuteLogout);
            ToggleChangePasswordCommand = new RelayCommand(() => IsChangePasswordExpanded = !IsChangePasswordExpanded);
            SavePasswordCommand = new RelayCommand(ExecuteSavePassword);

            LoadStatistics();
        }

        public void LoadStatistics()
        {
            if (AuthService.CurrentUser == null) return;

            var stats = _authService.GetUserStatistics(AuthService.CurrentUser.Id);
            TotalTransactions = stats.TotalTransactions;
            LifetimeIncome = stats.LifetimeIncome;
            LifetimeExpenses = stats.LifetimeExpenses;
            MemberSince = stats.MemberSince;
        }

        private void ExecuteSavePassword()
        {
            PasswordMessage = string.Empty;

            if (AuthService.CurrentUser == null) return;

            if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                PasswordMessage = "All password fields are required.";
                IsPasswordError = true;
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                PasswordMessage = "New passwords do not match.";
                IsPasswordError = true;
                return;
            }

            if (NewPassword.Length < 6)
            {
                PasswordMessage = "New password must be at least 6 characters.";
                IsPasswordError = true;
                return;
            }

            bool success = _authService.ChangePassword(AuthService.CurrentUser.Id, CurrentPassword, NewPassword, out string msg);
            PasswordMessage = msg;
            IsPasswordError = !success;

            if (success)
            {
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
                IsChangePasswordExpanded = false;
                _mainViewModel.ShowToast("Password updated successfully!");
            }
        }

        private void ExecuteLogout()
        {
            _mainViewModel.LogoutCommand.Execute(null);
        }
    }
}
