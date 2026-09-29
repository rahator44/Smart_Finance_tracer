// [Rahat Corporate Architecture] Enterprise Revenue, Margin, Burn Rate & Runway Metrics
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class OfficeDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TransactionService _transactionService;

        private DateTime _selectedMonth;
        private decimal _totalRevenue;
        private decimal _totalExpenses;
        private decimal _netProfit;
        private decimal _cashFlow;
        private double _revenueTrendPct;
        private double _expensesTrendPct;
        private string _searchText = string.Empty;
        private string _selectedTypeFilter = "All"; // "All", "Income", "Expense", "Transfer"
        private ObservableCollection<Transaction> _transactions;
        private List<DateTime> _monthOptions;
        private BudgetStatusReport _budgetStatus = new();
        private string _corporateInsight = string.Empty;

        public List<DateTime> MonthOptions
        {
            get => _monthOptions;
            set => SetProperty(ref _monthOptions, value);
        }

        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    OnPropertyChanged(nameof(SelectedMonthLabel));
                    LoadData();
                }
            }
        }

        public string SelectedMonthLabel => SelectedMonth.ToString("MMMM yyyy");

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }

        public decimal NetProfit
        {
            get => _netProfit;
            set => SetProperty(ref _netProfit, value);
        }

        public decimal CashFlow
        {
            get => _cashFlow;
            set => SetProperty(ref _cashFlow, value);
        }

        public double RevenueTrendPct
        {
            get => _revenueTrendPct;
            set => SetProperty(ref _revenueTrendPct, value);
        }

        public double ExpensesTrendPct
        {
            get => _expensesTrendPct;
            set => SetProperty(ref _expensesTrendPct, value);
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
                    OnPropertyChanged(nameof(IsIncomeFilterSelected));
                    OnPropertyChanged(nameof(IsExpenseFilterSelected));
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

        public bool IsIncomeFilterSelected
        {
            get => SelectedTypeFilter == "Income";
            set { if (value) SelectedTypeFilter = "Income"; }
        }

        public bool IsExpenseFilterSelected
        {
            get => SelectedTypeFilter == "Expense";
            set { if (value) SelectedTypeFilter = "Expense"; }
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

        public string CorporateInsight
        {
            get => _corporateInsight;
            set => SetProperty(ref _corporateInsight, value);
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

        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand AddTransactionNavCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand SetFilterCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public OfficeDashboardViewModel(MainViewModel mainViewModel, TransactionService transactionService, DateTime? initialMonth = null)
        {
            _mainViewModel = mainViewModel;
            _transactionService = transactionService;
            _transactions = new ObservableCollection<Transaction>();
            _monthOptions = new List<DateTime>();

            DeleteCommand = new RelayCommand(ExecuteDelete);
            EditCommand = new RelayCommand(ExecuteEdit);
            AddTransactionNavCommand = new RelayCommand(() => _mainViewModel.NavigateAddCommand.Execute(null));
            PreviousMonthCommand = new RelayCommand(ExecutePreviousMonth);
            NextMonthCommand = new RelayCommand(ExecuteNextMonth);
            SetFilterCommand = new RelayCommand(param => SelectedTypeFilter = param?.ToString() ?? "All");
            ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

            InitializeMonths(initialMonth);
            LoadData();
        }

        private void InitializeMonths(DateTime? targetMonth = null)
        {
            if (AuthService.CurrentUser == null) return;
            MonthOptions = _transactionService.GetAvailableMonths(AuthService.CurrentUser.Id, isOffice: true);

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

            // 1. Calculate dynamic business KPIs directly from database
            var summary = _transactionService.GetMonthlySummary(userId, SelectedMonth, isOffice: true);
            TotalRevenue = summary.TotalRevenue;
            TotalExpenses = summary.TotalExpenses;
            NetProfit = summary.NetProfit;
            CashFlow = summary.CashFlow;

            RevenueTrendPct = summary.IncomeChangePercentage;
            ExpensesTrendPct = summary.ExpensesChangePercentage;

            // 2. Load Corporate Budget Status
            BudgetStatus = _transactionService.GetBudgetStatus(userId, SelectedMonth, isOffice: true);

            // 3. Generate Corporate Financial Insight
            var breakdown = _transactionService.GetCategoryBreakdown(userId, SelectedMonth, isOffice: true, "Expense");
            if (breakdown.Count > 0)
            {
                var top = breakdown[0];
                CorporateInsight = $"Primary Operating Cost: {top.Category} accounts for {top.Percentage:F0}% (${top.TotalAmount:N0}) of monthly burn.";
            }
            else if (summary.TotalRevenue > 0)
            {
                CorporateInsight = $"Monthly revenue recorded: ${summary.TotalRevenue:N0} with no operating expenses registered.";
            }
            else
            {
                CorporateInsight = "No corporate financial records found for this period.";
            }

            // 4. Filter and display transactions
            FilterTransactions();
        }

        private void FilterTransactions()
        {
            if (AuthService.CurrentUser == null) return;

            var rawTxs = _transactionService.GetFilteredTransactions(
                AuthService.CurrentUser.Id,
                SelectedMonth,
                isOffice: true,
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
                    $"Are you sure you want to permanently delete this corporate transaction ({tx.Category}) of {Math.Abs(tx.Amount):C0}?", 
                    "Confirm Business Delete", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = _transactionService.DeleteTransaction(tx.Id, AuthService.CurrentUser.Id);
                    if (deleted)
                    {
                        _mainViewModel.ShowToast($"Corporate transaction '{tx.Category}' deleted.");
                        LoadData();
                    }
                    else
                    {
                        _mainViewModel.ShowToast("Could not delete corporate transaction.", isError: true);
                    }
                }
            }
        }

        private void ExecuteEdit(object? parameter)
        {
            if (parameter is Transaction tx)
            {
                var editViewModel = new AddTransactionViewModel(_mainViewModel, _transactionService, isOffice: true, tx);
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
