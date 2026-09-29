using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Services;
using SmartFinanceManager.Views.Windows;

namespace SmartFinanceManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly TransactionService _transactionService;
        private readonly SubscriptionService _subscriptionService;

        private object _currentView = null!;
        private string _activeTab = "Home"; // "Home", "Charts", "Add", "Reports", "Profile", "Admin"
        private bool _isOfficeMode;

        // Global Toast Notification State
        private string _toastMessage = string.Empty;
        private bool _isToastVisible;
        private bool _isToastError;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string ActiveTab
        {
            get => _activeTab;
            set
            {
                if (SetProperty(ref _activeTab, value))
                {
                    OnPropertyChanged(nameof(IsHomeActive));
                    OnPropertyChanged(nameof(IsChartsActive));
                    OnPropertyChanged(nameof(IsReportsActive));
                    OnPropertyChanged(nameof(IsProfileActive));
                    OnPropertyChanged(nameof(IsAdminActive));
                }
            }
        }

        public bool IsHomeActive => ActiveTab == "Home";
        public bool IsChartsActive => ActiveTab == "Charts";
        public bool IsReportsActive => ActiveTab == "Reports";
        public bool IsProfileActive => ActiveTab == "Profile";
        public bool IsAdminActive => ActiveTab == "Admin";

        public string FullName => AuthService.CurrentUser?.FullName ?? "Guest";
        public string Email => AuthService.CurrentUser?.Email ?? "guest@finance.com";
        public string AccountType => AuthService.CurrentUser?.AccountType ?? "Personal";
        
        public bool IsAdmin => AdminConfig.IsAdmin(AuthService.CurrentUser);
        public bool IsOfficeUser => string.Equals(AccountType, "Office", StringComparison.OrdinalIgnoreCase);

        public bool IsOfficeMode
        {
            get => _isOfficeMode;
            set
            {
                if (SetProperty(ref _isOfficeMode, value))
                {
                    OnPropertyChanged(nameof(CurrentModeLabel));
                }
            }
        }

        public string CurrentModeLabel => IsOfficeMode ? "Office Mode" : "Personal Mode";

        public string ToastMessage
        {
            get => _toastMessage;
            set => SetProperty(ref _toastMessage, value);
        }

        public bool IsToastVisible
        {
            get => _isToastVisible;
            set => SetProperty(ref _isToastVisible, value);
        }

        public bool IsToastError
        {
            get => _isToastError;
            set => SetProperty(ref _isToastError, value);
        }

        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateChartsCommand { get; }
        public ICommand NavigateAddCommand { get; }
        public ICommand NavigateReportsCommand { get; }
        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateAdminCommand { get; }
        public ICommand SwitchModeCommand { get; }
        public ICommand OpenSubscriptionCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DismissToastCommand { get; }

        public event Action? RequestClose;

        public MainViewModel()
        {
            _authService = new AuthService();
            _transactionService = new TransactionService();
            _subscriptionService = new SubscriptionService();

            _isOfficeMode = IsOfficeUser;

            NavigateHomeCommand = new RelayCommand(NavigateHome);
            NavigateChartsCommand = new RelayCommand(NavigateCharts);
            NavigateAddCommand = new RelayCommand(NavigateAdd);
            NavigateReportsCommand = new RelayCommand(NavigateReports);
            NavigateProfileCommand = new RelayCommand(NavigateProfile);
            NavigateAdminCommand = new RelayCommand(NavigateAdmin);
            SwitchModeCommand = new RelayCommand(ExecuteSwitchMode);
            OpenSubscriptionCommand = new RelayCommand(ExecuteOpenSubscription);
            LogoutCommand = new RelayCommand(Logout);
            DismissToastCommand = new RelayCommand(() => IsToastVisible = false);

            // Initialize to correct dashboard with strict subscription verification
            NavigateHome();
        }

        public void ShowToast(string message, bool isError = false)
        {
            ToastMessage = message;
            IsToastError = isError;
            IsToastVisible = true;

            // Auto dismiss after 3.5 seconds
            Task.Delay(3500).ContinueWith(_ =>
            {
                if (ToastMessage == message)
                {
                    IsToastVisible = false;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void NavigateHome(DateTime? targetMonth = null)
        {
            ActiveTab = "Home";

            if (IsOfficeMode)
            {
                // Strict authorization check for Office Finance access
                if (_subscriptionService.HasOfficeAccess(AuthService.CurrentUser))
                {
                    CurrentView = new OfficeDashboardViewModel(this, _transactionService, targetMonth);
                }
                else if (IsOfficeUser)
                {
                    // Office user without approved subscription: do not redirect to personal account
                    Logout();
                }
                else
                {
                    // Personal user attempting to access office mode without approved subscription
                    IsOfficeMode = false;
                    CurrentView = new PersonalDashboardViewModel(this, _transactionService, _subscriptionService, targetMonth);
                    ShowToast("Office Finance requires an approved subscription. Please subscribe to unlock.", isError: true);
                    ExecuteOpenSubscription();
                }
            }
            else
            {
                CurrentView = new PersonalDashboardViewModel(this, _transactionService, _subscriptionService, targetMonth);
            }
        }

        private void NavigateCharts()
        {
            ActiveTab = "Charts";
            bool isOffice = IsOfficeMode && _subscriptionService.HasOfficeAccess(AuthService.CurrentUser);
            CurrentView = new ChartsViewModel(_transactionService, isOffice);
        }

        private void NavigateAdd()
        {
            ActiveTab = "Add";
            bool isOffice = IsOfficeMode && _subscriptionService.HasOfficeAccess(AuthService.CurrentUser);
            CurrentView = new AddTransactionViewModel(this, _transactionService, isOffice);
        }

        private void NavigateReports()
        {
            ActiveTab = "Reports";
            bool isOffice = IsOfficeMode && _subscriptionService.HasOfficeAccess(AuthService.CurrentUser);
            CurrentView = new ReportsViewModel(_transactionService, isOffice);
        }

        private void NavigateProfile()
        {
            ActiveTab = "Profile";
            CurrentView = new ProfileViewModel(this, _authService);
        }

        public void NavigateAdmin()
        {
            if (!IsAdmin)
            {
                ShowToast("Unauthorized: Administrator access required.", isError: true);
                return;
            }

            ActiveTab = "Admin";
            CurrentView = new AdminDashboardViewModel(this, _subscriptionService);
        }

        private void ExecuteSwitchMode()
        {
            bool targetOffice = !IsOfficeMode;

            if (targetOffice)
            {
                if (!_subscriptionService.HasOfficeAccess(AuthService.CurrentUser))
                {
                    ShowToast("Office Finance requires an approved subscription.", isError: true);
                    ExecuteOpenSubscription();
                    return;
                }
            }

            IsOfficeMode = targetOffice;
            NavigateHome();
            ShowToast($"Switched to {CurrentModeLabel}.");
        }

        private void ExecuteOpenSubscription()
        {
            var subWin = new SubscriptionWindow(_subscriptionService, AuthService.CurrentUser);
            subWin.ShowDialog();
            
            // If currently on Personal Dashboard, refresh its status
            if (CurrentView is PersonalDashboardViewModel personalVm)
            {
                personalVm.LoadData();
            }
        }

        private void Logout()
        {
            _authService.Logout();
            RequestClose?.Invoke();
        }

        protected override void ExecuteToggleTheme()
        {
            base.ExecuteToggleTheme();
            ShowToast($"Switched to {(ThemeService.IsLightTheme ? "Light" : "Dark")} Mode.");
        }
    }
}
