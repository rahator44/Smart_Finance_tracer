using System;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Services;
using SmartFinanceManager.Data;
using SmartFinanceManager.Models;
using System.Linq;

namespace SmartFinanceManager.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly SubscriptionService _subscriptionService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _rememberMe = true;
        private bool _isPasswordVisible;

        // Demo selection state (no email shown in input box)
        private bool _isPersonalDemoSelected;
        private bool _isOfficeDemoSelected;

        // Custom Demo Generation state
        private bool _isAddingSeedUser;
        private string _newSeedName = string.Empty;
        private string _newSeedType = "Personal"; // "Personal" or "Office"

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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public bool IsPersonalDemoSelected
        {
            get => _isPersonalDemoSelected;
            set => SetProperty(ref _isPersonalDemoSelected, value);
        }

        public bool IsOfficeDemoSelected
        {
            get => _isOfficeDemoSelected;
            set => SetProperty(ref _isOfficeDemoSelected, value);
        }

        public bool IsAddingSeedUser
        {
            get => _isAddingSeedUser;
            set => SetProperty(ref _isAddingSeedUser, value);
        }

        public string NewSeedName
        {
            get => _newSeedName;
            set => SetProperty(ref _newSeedName, value);
        }

        public string NewSeedType
        {
            get => _newSeedType;
            set => SetProperty(ref _newSeedType, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateRegisterCommand { get; }
        public ICommand SelectPersonalDemoCommand { get; }
        public ICommand SelectOfficeDemoCommand { get; }
        public ICommand ToggleAddSeedUserCommand { get; }
        public ICommand CreateSeedUserCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        public event Action? RequestClose;
        public event Action? RequestOpenRegister;
        public event Action? RequestOpenMain;
        public event Action<User>? RequestOpenSubscription;

        public LoginViewModel()
        {
            _authService = new AuthService();
            _subscriptionService = new SubscriptionService();
            LoginCommand = new RelayCommand(ExecuteLogin);
            NavigateRegisterCommand = new RelayCommand(ExecuteNavigateRegister);
            SelectPersonalDemoCommand = new RelayCommand(ExecuteSelectPersonalDemo);
            SelectOfficeDemoCommand = new RelayCommand(ExecuteSelectOfficeDemo);
            ToggleAddSeedUserCommand = new RelayCommand(() => IsAddingSeedUser = !IsAddingSeedUser);
            CreateSeedUserCommand = new RelayCommand(ExecuteCreateSeedUser);
            TogglePasswordVisibilityCommand = new RelayCommand(() => IsPasswordVisible = !IsPasswordVisible);

            // Default selection: Personal Demo
            IsPersonalDemoSelected = true;
        }

        private void ExecuteSelectPersonalDemo()
        {
            IsPersonalDemoSelected = true;
            IsOfficeDemoSelected = false;
            ErrorMessage = string.Empty;
        }

        private void ExecuteSelectOfficeDemo()
        {
            IsPersonalDemoSelected = false;
            IsOfficeDemoSelected = true;
            ErrorMessage = string.Empty;
        }

        private void ExecuteCreateSeedUser()
        {
            ErrorMessage = string.Empty;
            string profileName = string.IsNullOrWhiteSpace(NewSeedName) ? $"{NewSeedType} Demo User" : NewSeedName.Trim();
            string generatedEmail = $"demo_{NewSeedType.ToLower()}_{Guid.NewGuid().ToString().Substring(0, 5)}@gmail.com";

            using (var context = new FinanceDbContext())
            {
                var seedUser = new User
                {
                    FullName = profileName,
                    Email = generatedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    AccountType = NewSeedType,
                    CreatedDate = DateTime.UtcNow
                };

                context.Users.Add(seedUser);
                context.SaveChanges();

                var targetDate = DateTime.Today;
                if (NewSeedType == "Personal")
                {
                    context.Transactions.AddRange(
                        new Transaction { UserId = seedUser.Id, Type = "Income", Category = "Salary", Amount = 2500, Date = targetDate, IsOffice = false, Description = "Monthly wage" },
                        new Transaction { UserId = seedUser.Id, Type = "Expense", Category = "Food", Amount = -350, Date = targetDate, IsOffice = false, Description = "Groceries & Dining" }
                    );

                    context.Budgets.Add(new Budget { UserId = seedUser.Id, Month = targetDate.ToString("yyyy-MM"), TargetAmount = 1500, IsOffice = false });
                    IsPersonalDemoSelected = true;
                    IsOfficeDemoSelected = false;
                }
                else
                {
                    context.Transactions.AddRange(
                        new Transaction { UserId = seedUser.Id, Type = "Income", Category = "Sales", Amount = 18000, Date = targetDate, IsOffice = true, Description = "Enterprise Client Retainer" },
                        new Transaction { UserId = seedUser.Id, Type = "Expense", Category = "Rent", Amount = -3500, Date = targetDate, IsOffice = true, Description = "Commercial Office Space" }
                    );

                    context.Budgets.Add(new Budget { UserId = seedUser.Id, Month = targetDate.ToString("yyyy-MM"), TargetAmount = 20000, IsOffice = true });
                    IsPersonalDemoSelected = false;
                    IsOfficeDemoSelected = true;
                }

                context.SaveChanges();

                NewSeedName = string.Empty;
                IsAddingSeedUser = false;
                ErrorMessage = $"New {NewSeedType} demo profile generated!";
            }
        }

        private void ExecuteLogin()
        {
            ErrorMessage = string.Empty;
            string cleanEmail = Email?.Trim() ?? string.Empty;
            string expectedMode = IsOfficeDemoSelected ? "Office" : "Personal";

            if (!string.IsNullOrWhiteSpace(cleanEmail))
            {
                // Manual email login (supports admin, personal, and office accounts)
                if (!_authService.Login(cleanEmail, Password, RememberMe, out string message))
                {
                    ErrorMessage = message;
                    return;
                }
            }
            else if (IsPersonalDemoSelected)
            {
                // Login as Personal Demo
                if (!_authService.Login("personal@finance.com", "password123", "Personal", RememberMe, out string message))
                {
                    ErrorMessage = message;
                    return;
                }
            }
            else if (IsOfficeDemoSelected)
            {
                // Login as Office Demo
                if (!_authService.Login("office@finance.com", "password123", "Office", RememberMe, out string message))
                {
                    ErrorMessage = message;
                    return;
                }
            }
            else
            {
                ErrorMessage = "Please select a Mode (Personal/Office) or enter your Email and Password.";
                return;
            }

            var loggedInUser = AuthService.CurrentUser;
            if (loggedInUser != null && string.Equals(loggedInUser.AccountType, "Office", StringComparison.OrdinalIgnoreCase))
            {
                // Strict check: Office accounts require an active approved subscription
                if (!_subscriptionService.HasOfficeAccess(loggedInUser))
                {
                    // Do not redirect to personal account.
                    // Clear active session and open the subscription portal directly from the login page.
                    _authService.Logout();

                    var status = _subscriptionService.GetSubscriptionStatus(loggedInUser.Id);
                    if (status == SubscriptionStatus.Pending)
                    {
                        ErrorMessage = "Your Office Finance subscription verification is pending administrator approval.";
                    }
                    else if (status == SubscriptionStatus.Rejected)
                    {
                        ErrorMessage = "Your previous Office subscription request was rejected. Please resubmit verification.";
                    }
                    else
                    {
                        ErrorMessage = "Office Finance requires an approved subscription. Please subscribe to unlock.";
                    }

                    RequestOpenSubscription?.Invoke(loggedInUser);
                    return;
                }
            }

            RequestOpenMain?.Invoke();
            RequestClose?.Invoke();
        }

        private void ExecuteNavigateRegister()
        {
            RequestOpenRegister?.Invoke();
            RequestClose?.Invoke();
        }
    }
}
