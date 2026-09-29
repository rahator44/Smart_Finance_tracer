using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;
using SmartFinanceManager.Views.Windows;

namespace SmartFinanceManager.ViewModels
{
    public class PersonalDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TransactionService _transactionService;
        private readonly SubscriptionService _subscriptionService;

        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _totalTransfers;
        private decimal _balance;
        private DateTime _selectedMonth;
        private List<DateTime> _monthOptions;
        private string _selectedTypeFilter = "All";
        private string _searchText = string.Empty;
        private BudgetStatusReport _budgetStatus = new();
        private string _financialInsight = string.Empty;
        private ObservableCollection<Transaction> _transactions;

        // Subscription Promotional State
        private bool _isOfficePromoVisible;
        private bool _isOfficePromoPending;
        private SubscriptionStatus? _subscriptionStatus;

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }

        public decimal TotalTransfers
        {
            get => _totalTransfers;
            set => SetProperty(ref _totalTransfers, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    OnPropertyChanged(nameof(SelectedMonthDisplay));
                    LoadData();
                }
            }
        }

        public string SelectedMonthDisplay => SelectedMonth.ToString("MMMM yyyy");

        public List<DateTime> MonthOptions
        {
            get => _monthOptions;
            set => SetProperty(ref _monthOptions, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterTransactions();
                }
            }
        }

        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                if (SetProperty(ref _selectedTypeFilter, value))
                {
                    OnPropertyChanged(nameof(IsAllFilterSelected));
                    OnPropertyChanged(nameof(IsExpenseFilterSelected));
                    OnPropertyChanged(nameof(IsIncomeFilterSelected));
                    OnPropertyChanged(nameof(IsTransferFilterSelected));
                    FilterTransactions();
                }
            }
        }

        public bool IsAllFilterSelected
        {
            get => SelectedTypeFilter == "All";
            set { if (value) SelectedTypeFilter = "All"; }
        }

        public bool IsExpenseFilterSelected
        {
            get => SelectedTypeFilter == "Expense";
            set { if (value) SelectedTypeFilter = "Expense"; }
        }

        public bool IsIncomeFilterSelected
        {
            get => SelectedTypeFilter == "Income";
            set { if (value) SelectedTypeFilter = "Income"; }
        }

        public bool IsTransferFilterSelected
        {
            get => SelectedTypeFilter == "Transfer";
            set { if (value) SelectedTypeFilter = "Transfer"; }
        }

        public BudgetStatusReport BudgetStatus
        {
            get => _budgetStatus;
            set => SetProperty(ref _budgetStatus, value);
        }

        public string FinancialInsight
        {
            get => _financialInsight;
            set => SetProperty(ref _financialInsight, value);
        }

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                if (SetProperty(ref _transactions, value))
                {
                    OnPropertyChanged(nameof(HasTransactions));
                }
            }
        }

        public bool HasTransactions => Transactions != null && Transactions.Count > 0;

        public bool IsOfficePromoVisible
        {
            get => _isOfficePromoVisible;
            set => SetProperty(ref _isOfficePromoVisible, value);
        }

        public bool IsOfficePromoPending
        {
            get => _isOfficePromoPending;
            set => SetProperty(ref _isOfficePromoPending, value);
        }

        public SubscriptionStatus? SubscriptionStatus
        {
            get => _subscriptionStatus;
            set => SetProperty(ref _subscriptionStatus, value);
        }

        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand AddTransactionNavCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand SetFilterCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand OpenSubscriptionCommand { get; }

        public PersonalDashboardViewModel(MainViewModel mainViewModel, TransactionService transactionService, SubscriptionService? subscriptionService = null, DateTime? initialMonth = null)
        {
            _mainViewModel = mainViewModel;
            _transactionService = transactionService;
            _subscriptionService = subscriptionService ?? new SubscriptionService();
            _transactions = new ObservableCollection<Transaction>();
            _monthOptions = new List<DateTime>();

            DeleteCommand = new RelayCommand(ExecuteDelete);
            EditCommand = new RelayCommand(ExecuteEdit);
            AddTransactionNavCommand = new RelayCommand(() => _mainViewModel.NavigateAddCommand.Execute(null));
            PreviousMonthCommand = new RelayCommand(ExecutePreviousMonth);
            NextMonthCommand = new RelayCommand(ExecuteNextMonth);
            SetFilterCommand = new RelayCommand(param => SelectedTypeFilter = param?.ToString() ?? "All");
            ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
            OpenSubscriptionCommand = new RelayCommand(ExecuteOpenSubscription);

            InitializeMonths(initialMonth);
            LoadData();
        }

        private void InitializeMonths(DateTime? targetMonth = null)
        {
            if (AuthService.CurrentUser == null) return;
            MonthOptions = _transactionService.GetAvailableMonths(AuthService.CurrentUser.Id, isOffice: false);

            var todayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime preferred = targetMonth ?? todayMonth;

            var match = MonthOptions.FirstOrDefault(m => m.Year == preferred.Year && m.Month == preferred.Month);
            if (match != default)
            {
                _selectedMonth = match;
            }
            else if (MonthOptions.Contains(todayMonth))
            {
                _selectedMonth = todayMonth;
            }
            else
            {
                _selectedMonth = MonthOptions.FirstOrDefault();
            }
        }

        public void LoadData()
        {
            if (AuthService.CurrentUser == null) return;

            int userId = AuthService.CurrentUser.Id;

            // 1. Calculate dynamic KPIs directly from database
            var summary = _transactionService.GetMonthlySummary(userId, SelectedMonth, isOffice: false);
            TotalIncome = summary.TotalIncome;
            TotalExpenses = summary.TotalExpenses;
            TotalTransfers = summary.TotalTransfers;
            Balance = summary.Balance;

            // 2. Load Budget Alert & Health
            BudgetStatus = _transactionService.GetBudgetStatus(userId, SelectedMonth, isOffice: false);

            // 3. Generate Smart Financial Insight
            var breakdown = _transactionService.GetCategoryBreakdown(userId, SelectedMonth, isOffice: false, "Expense");
            if (breakdown.Count > 0)
            {
                var top = breakdown[0];
                FinancialInsight = $"Top Expense: {top.Category} makes up {top.Percentage:F0}% (${top.TotalAmount:N0}) of monthly spending.";
            }
            else if (summary.TotalIncome > 0)
            {
                FinancialInsight = $"Income recorded: ${summary.TotalIncome:N0}. No expenses recorded yet for this month.";
            }
            else
            {
                FinancialInsight = "No financial activity recorded yet for this month.";
            }

            // 4. Check Office Subscription State for Promotional Card
            if (AdminConfig.IsAdmin(AuthService.CurrentUser))
            {
                IsOfficePromoVisible = false;
            }
            else
            {
                SubscriptionStatus = _subscriptionService.GetSubscriptionStatus(userId);
                if (SubscriptionStatus == Models.SubscriptionStatus.Approved)
                {
                    IsOfficePromoVisible = false;
                }
                else if (SubscriptionStatus == Models.SubscriptionStatus.Pending)
                {
                    IsOfficePromoVisible = true;
                    IsOfficePromoPending = true;
                }
                else
                {
                    IsOfficePromoVisible = true;
                    IsOfficePromoPending = false;
                }
            }

            // 5. Filter and display transactions
            FilterTransactions();
        }

        private void ExecuteOpenSubscription()
        {
            var subWin = new SubscriptionWindow(_subscriptionService, AuthService.CurrentUser);
            subWin.ShowDialog();
            LoadData();
        }

        private void FilterTransactions()
        {
            if (AuthService.CurrentUser == null) return;

            var rawTxs = _transactionService.GetFilteredTransactions(
                AuthService.CurrentUser.Id,
                SelectedMonth,
                isOffice: false,
                typeFilter: SelectedTypeFilter,
                categoryFilter: null,
                searchTerm: SearchText);

            Transactions = new ObservableCollection<Transaction>(rawTxs);
        }

        private void ExecutePreviousMonth()
        {
            int currentIndex = MonthOptions.FindIndex(m => m.Year == SelectedMonth.Year && m.Month == SelectedMonth.Month);
            if (currentIndex != -1 && currentIndex < MonthOptions.Count - 1)
            {
                SelectedMonth = MonthOptions[currentIndex + 1];
            }
            else
            {
                var newMonth = SelectedMonth.AddMonths(-1);
                var updated = new List<DateTime>(MonthOptions) { newMonth };
                MonthOptions = updated.OrderByDescending(d => d).ToList();
                SelectedMonth = newMonth;
            }
        }

        private void ExecuteNextMonth()
        {
            int currentIndex = MonthOptions.FindIndex(m => m.Year == SelectedMonth.Year && m.Month == SelectedMonth.Month);
            if (currentIndex > 0)
            {
                SelectedMonth = MonthOptions[currentIndex - 1];
            }
            else
            {
                var newMonth = SelectedMonth.AddMonths(1);
                var updated = new List<DateTime>(MonthOptions) { newMonth };
                MonthOptions = updated.OrderByDescending(d => d).ToList();
                SelectedMonth = newMonth;
            }
        }

        private void ExecuteDelete(object? parameter)
        {
            if (parameter is Transaction tx && AuthService.CurrentUser != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to permanently delete this {tx.Category} transaction of {Math.Abs(tx.Amount):C0}?", 
                    "Confirm Delete", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = _transactionService.DeleteTransaction(tx.Id, AuthService.CurrentUser.Id);
                    if (deleted)
                    {
                        _mainViewModel.ShowToast($"Transaction '{tx.Category}' deleted.");
                        LoadData();
                    }
                    else
                    {
                        _mainViewModel.ShowToast("Could not delete transaction.", isError: true);
                    }
                }
            }
        }

        private void ExecuteEdit(object? parameter)
        {
            if (parameter is Transaction tx)
            {
                var editViewModel = new AddTransactionViewModel(_mainViewModel, _transactionService, isOffice: false, tx);
                _mainViewModel.CurrentView = editViewModel;
                _mainViewModel.ActiveTab = "Add";
            }
        }

        protected override void ExecuteToggleTheme()
        {
            base.ExecuteToggleTheme();
            _mainViewModel?.ShowToast($"Switched to {(ThemeService.IsLightTheme ? "Light" : "Dark")} Mode.");
        }
    }
}
