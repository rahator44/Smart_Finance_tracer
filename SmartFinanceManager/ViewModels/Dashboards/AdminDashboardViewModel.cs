using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class UserAdminItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "USER";
        public string AccountType { get; set; } = "Personal";
        public DateTime CreatedDate { get; set; }
        public string SubscriptionStatusText { get; set; } = "None";
        public SubscriptionStatus? SubscriptionStatus { get; set; }
    }

    public class AdminDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly SubscriptionService _subscriptionService;

        private AdminOverviewStatistics _stats = new();
        private string _activeSection = "Overview"; // "Overview", "Subscriptions", "Users"
        private string _subscriptionFilter = "All"; // "All", "Pending", "Approved", "Rejected"
        private string _searchQuery = string.Empty;

        // Selected Subscription for Details / Action Modal
        private OfficeSubscription? _selectedSubscription;
        private string _adminNoteInput = string.Empty;
        private bool _isActionModalVisible;

        // Add Site Operating Expense Modal State
        private bool _isAddExpenseModalVisible;
        private string _expenseAmountInput = string.Empty;
        private string _expenseCategoryInput = "Cloud Server & Hosting";
        private string _expenseDescriptionInput = string.Empty;
        private DateTime _expenseDateInput = DateTime.Today;
        private string _addExpenseErrorMessage = string.Empty;

        public AdminOverviewStatistics Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        public string ActiveSection
        {
            get => _activeSection;
            set
            {
                if (SetProperty(ref _activeSection, value))
                {
                    OnPropertyChanged(nameof(IsOverviewActive));
                    OnPropertyChanged(nameof(IsSubscriptionsActive));
                    OnPropertyChanged(nameof(IsUsersActive));
                }
            }
        }

        public bool IsOverviewActive
        {
            get => ActiveSection == "Overview";
            set
            {
                if (value) ActiveSection = "Overview";
            }
        }

        public bool IsSubscriptionsActive
        {
            get => ActiveSection == "Subscriptions";
            set
            {
                if (value) ActiveSection = "Subscriptions";
            }
        }

        public bool IsUsersActive
        {
            get => ActiveSection == "Users";
            set
            {
                if (value) ActiveSection = "Users";
            }
        }

        public string SubscriptionFilter
        {
            get => _subscriptionFilter;
            set
            {
                if (SetProperty(ref _subscriptionFilter, value))
                {
                    ApplySubscriptionFilter();
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplySubscriptionFilter();
                    ApplyUserFilter();
                }
            }
        }

        public ObservableCollection<OfficeSubscription> FilteredSubscriptions { get; } = new();
        public ObservableCollection<UserAdminItem> FilteredUsers { get; } = new();
        public ObservableCollection<Transaction> SiteExpensesList { get; } = new();

        public List<string> ExpenseCategories { get; } = new()
        {
            "Cloud Server & Hosting",
            "Domain & DNS",
            "SMS & Email Gateway",
            "Maintenance & DevOps",
            "Marketing & Growth",
            "API & Third-party Services",
            "Office Operations",
            "Security & Backups",
            "Miscellaneous Overhead"
        };

        public OfficeSubscription? SelectedSubscription
        {
            get => _selectedSubscription;
            set => SetProperty(ref _selectedSubscription, value);
        }

        public string AdminNoteInput
        {
            get => _adminNoteInput;
            set => SetProperty(ref _adminNoteInput, value);
        }

        public bool IsActionModalVisible
        {
            get => _isActionModalVisible;
            set => SetProperty(ref _isActionModalVisible, value);
        }

        public bool IsAddExpenseModalVisible
        {
            get => _isAddExpenseModalVisible;
            set => SetProperty(ref _isAddExpenseModalVisible, value);
        }

        public string ExpenseAmountInput
        {
            get => _expenseAmountInput;
            set => SetProperty(ref _expenseAmountInput, value);
        }

        public string ExpenseCategoryInput
        {
            get => _expenseCategoryInput;
            set => SetProperty(ref _expenseCategoryInput, value);
        }

        public string ExpenseDescriptionInput
        {
            get => _expenseDescriptionInput;
            set => SetProperty(ref _expenseDescriptionInput, value);
        }

        public DateTime ExpenseDateInput
        {
            get => _expenseDateInput;
            set => SetProperty(ref _expenseDateInput, value);
        }

        public string AddExpenseErrorMessage
        {
            get => _addExpenseErrorMessage;
            set => SetProperty(ref _addExpenseErrorMessage, value);
        }

        public ICommand SelectSectionCommand { get; }
        public ICommand FilterSubscriptionsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewSubscriptionCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand CloseModalCommand { get; }

        // Operating Expense Commands
        public ICommand OpenAddExpenseModalCommand { get; }
        public ICommand CloseAddExpenseModalCommand { get; }
        public ICommand SaveSiteExpenseCommand { get; }
        public ICommand DeleteSiteExpenseCommand { get; }

        private List<OfficeSubscription> _allSubscriptions = new();
        private List<UserAdminItem> _allUsers = new();

        public AdminDashboardViewModel(MainViewModel mainViewModel, SubscriptionService? subscriptionService = null)
        {
            _mainViewModel = mainViewModel;
            _subscriptionService = subscriptionService ?? new SubscriptionService();

            SelectSectionCommand = new RelayCommand(p =>
            {
                if (p is string sec) ActiveSection = sec;
            });

            FilterSubscriptionsCommand = new RelayCommand(p =>
            {
                if (p is string f) SubscriptionFilter = f;
            });

            RefreshCommand = new RelayCommand(LoadData);
            ViewSubscriptionCommand = new RelayCommand(ExecuteViewSubscription);
            ApproveCommand = new RelayCommand(ExecuteApprove);
            RejectCommand = new RelayCommand(ExecuteReject);
            CloseModalCommand = new RelayCommand(() => IsActionModalVisible = false);

            OpenAddExpenseModalCommand = new RelayCommand(() =>
            {
                ExpenseAmountInput = string.Empty;
                ExpenseCategoryInput = "Cloud Server & Hosting";
                ExpenseDescriptionInput = string.Empty;
                ExpenseDateInput = DateTime.Today;
                AddExpenseErrorMessage = string.Empty;
                IsAddExpenseModalVisible = true;
            });

            CloseAddExpenseModalCommand = new RelayCommand(() => IsAddExpenseModalVisible = false);
            SaveSiteExpenseCommand = new RelayCommand(ExecuteSaveSiteExpense);
            DeleteSiteExpenseCommand = new RelayCommand(ExecuteDeleteSiteExpense);

            LoadData();
        }

        public void LoadData()
        {
            Stats = _subscriptionService.GetAdminOverviewStatistics();

            SiteExpensesList.Clear();
            foreach (var exp in Stats.SiteExpenses)
            {
                SiteExpensesList.Add(exp);
            }

            _allSubscriptions = _subscriptionService.GetAllSubscriptions();
            ApplySubscriptionFilter();

            var users = _subscriptionService.GetAllUsers();
            _allUsers = users.Select(u =>
            {
                var userSub = _allSubscriptions.FirstOrDefault(s => s.UserId == u.Id);
                string subText = "None";
                if (AdminConfig.IsAdmin(u))
                {
                    subText = "N/A (Admin)";
                }
                else if (userSub != null)
                {
                    subText = userSub.Status.ToString();
                }

                return new UserAdminItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = AdminConfig.IsAdmin(u) ? "ADMIN" : "USER",
                    AccountType = u.AccountType,
                    CreatedDate = u.CreatedDate,
                    SubscriptionStatus = userSub?.Status,
                    SubscriptionStatusText = subText
                };
            }).ToList();

            ApplyUserFilter();
        }

        private void ApplySubscriptionFilter()
        {
            var query = _allSubscriptions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SubscriptionFilter) && SubscriptionFilter != "All")
            {
                if (Enum.TryParse<SubscriptionStatus>(SubscriptionFilter, out var status))
                {
                    query = query.Where(s => s.Status == status);
                }
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string q = SearchQuery.ToLowerInvariant();
                query = query.Where(s => 
                    s.Email.ToLowerInvariant().Contains(q) ||
                    s.TransactionId.ToLowerInvariant().Contains(q) ||
                    (s.User != null && s.User.FullName.ToLowerInvariant().Contains(q)));
            }

            FilteredSubscriptions.Clear();
            foreach (var s in query)
            {
                FilteredSubscriptions.Add(s);
            }
        }

        private void ApplyUserFilter()
        {
            var query = _allUsers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string q = SearchQuery.ToLowerInvariant();
                query = query.Where(u =>
                    u.FullName.ToLowerInvariant().Contains(q) ||
                    u.Email.ToLowerInvariant().Contains(q) ||
                    u.AccountType.ToLowerInvariant().Contains(q));
            }

            FilteredUsers.Clear();
            foreach (var u in query)
            {
                FilteredUsers.Add(u);
            }
        }

        private void ExecuteViewSubscription(object? parameter)
        {
            if (parameter is OfficeSubscription sub)
            {
                SelectedSubscription = sub;
                AdminNoteInput = sub.AdminNote ?? string.Empty;
                IsActionModalVisible = true;
            }
        }

        private void ExecuteApprove(object? parameter)
        {
            var sub = parameter as OfficeSubscription ?? SelectedSubscription;
            if (sub == null || AuthService.CurrentUser == null) return;

            bool success = _subscriptionService.ApproveSubscription(
                sub.Id, 
                AuthService.CurrentUser.Id, 
                AdminNoteInput, 
                out string message);

            if (success)
            {
                _mainViewModel.ShowToast($"Approved subscription for {sub.Email}.");
                IsActionModalVisible = false;
                LoadData();
            }
            else
            {
                _mainViewModel.ShowToast(message, isError: true);
            }
        }

        private void ExecuteReject(object? parameter)
        {
            var sub = parameter as OfficeSubscription ?? SelectedSubscription;
            if (sub == null || AuthService.CurrentUser == null) return;

            bool success = _subscriptionService.RejectSubscription(
                sub.Id, 
                AuthService.CurrentUser.Id, 
                AdminNoteInput, 
                out string message);

            if (success)
            {
                _mainViewModel.ShowToast($"Rejected subscription for {sub.Email}.");
                IsActionModalVisible = false;
                LoadData();
            }
            else
            {
                _mainViewModel.ShowToast(message, isError: true);
            }
        }

        private void ExecuteSaveSiteExpense()
        {
            AddExpenseErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ExpenseAmountInput))
            {
                AddExpenseErrorMessage = "Please enter the expense amount.";
                return;
            }

            if (!decimal.TryParse(ExpenseAmountInput, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) &&
                !decimal.TryParse(ExpenseAmountInput, out amount))
            {
                AddExpenseErrorMessage = "Please enter a valid numeric amount.";
                return;
            }

            if (amount <= 0)
            {
                AddExpenseErrorMessage = "Amount must be greater than 0.";
                return;
            }

            int adminId = AuthService.CurrentUser?.Id ?? 0;
            if (adminId <= 0)
            {
                AddExpenseErrorMessage = "Administrator account session not found.";
                return;
            }

            var (success, msg, _) = _subscriptionService.AddSiteOperatingExpense(
                adminId,
                amount,
                ExpenseCategoryInput,
                ExpenseDescriptionInput,
                ExpenseDateInput);

            if (success)
            {
                _mainViewModel.ShowToast($"Site operating expense of {amount:N0} BDT recorded.");
                IsAddExpenseModalVisible = false;
                LoadData();
            }
            else
            {
                AddExpenseErrorMessage = msg;
            }
        }

        private void ExecuteDeleteSiteExpense(object? parameter)
        {
            if (parameter is Transaction tx && AuthService.CurrentUser != null)
            {
                bool deleted = _subscriptionService.DeleteSiteOperatingExpense(tx.Id, AuthService.CurrentUser.Id);
                if (deleted)
                {
                    _mainViewModel.ShowToast("Operating expense record removed.");
                    LoadData();
                }
            }
        }
    }
}
