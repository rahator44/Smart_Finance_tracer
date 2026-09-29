// [Rahat Corporate Breakdown] Corporate monthly expenditure and operating budget breakdowns
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class CategorySpendingItem
    {
        public string Category { get; set; } = string.Empty;
        public decimal Spent { get; set; }
        public double PercentageOfTotal { get; set; }
        public string ColorBrush { get; set; } = "#007AFF";
    }

    public class ReportsViewModel : ViewModelBase
    {
        private readonly TransactionService _transactionService;
        private readonly bool _isOffice;

        private DateTime _selectedMonth;
        private decimal _income;
        private decimal _expenses;
        private decimal _balance;
        private decimal _budgetLimit;
        private decimal _remaining;
        private double _remainingPercentage;
        private double _usedPercentage;
        private BudgetHealth _budgetHealth = BudgetHealth.Healthy;
        private string _budgetStatusMessage = string.Empty;
        private string _budgetInputText = string.Empty;

        // Trends vs Previous Month
        private decimal _expensesDelta;
        private double _expensesChangePct;
        private decimal _incomeDelta;
        private double _incomeChangePct;

        private List<DateTime> _monthOptions;
        private ObservableCollection<CategorySpendingItem> _categoryBreakdown = new();

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
                    LoadReportsData();
                }
            }
        }

        public string SelectedMonthLabel => SelectedMonth.ToString("MMMM yyyy");

        public decimal Income
        {
            get => _income;
            set => SetProperty(ref _income, value);
        }

        public decimal Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public decimal BudgetLimit
        {
            get => _budgetLimit;
            set => SetProperty(ref _budgetLimit, value);
        }

        public decimal Remaining
        {
            get => _remaining;
            set => SetProperty(ref _remaining, value);
        }

        public double RemainingPercentage
        {
            get => _remainingPercentage;
            set => SetProperty(ref _remainingPercentage, value);
        }

        public double UsedPercentage
        {
            get => _usedPercentage;
            set => SetProperty(ref _usedPercentage, value);
        }

        public BudgetHealth BudgetHealth
        {
            get => _budgetHealth;
            set => SetProperty(ref _budgetHealth, value);
        }

        public string BudgetStatusMessage
        {
            get => _budgetStatusMessage;
            set => SetProperty(ref _budgetStatusMessage, value);
        }

        public string BudgetInputText
        {
            get => _budgetInputText;
            set => SetProperty(ref _budgetInputText, value);
        }

        public decimal ExpensesDelta
        {
            get => _expensesDelta;
            set => SetProperty(ref _expensesDelta, value);
        }

        public double ExpensesChangePct
        {
            get => _expensesChangePct;
            set => SetProperty(ref _expensesChangePct, value);
        }

        public decimal IncomeDelta
        {
            get => _incomeDelta;
            set => SetProperty(ref _incomeDelta, value);
        }

        public double IncomeChangePct
        {
            get => _incomeChangePct;
            set => SetProperty(ref _incomeChangePct, value);
        }

        public ObservableCollection<CategorySpendingItem> CategoryBreakdown
        {
            get => _categoryBreakdown;
            set => SetProperty(ref _categoryBreakdown, value);
        }

        public bool HasSpending => CategoryBreakdown != null && CategoryBreakdown.Count > 0;

        public ICommand SaveBudgetCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }

        public ReportsViewModel(TransactionService transactionService, bool isOffice)
        {
            _transactionService = transactionService;
            _isOffice = isOffice;
            _monthOptions = new List<DateTime>();

            SaveBudgetCommand = new RelayCommand(ExecuteSaveBudget);
            PreviousMonthCommand = new RelayCommand(ExecutePreviousMonth);
            NextMonthCommand = new RelayCommand(ExecuteNextMonth);

            InitializeMonths();
            LoadReportsData();
        }

        private void InitializeMonths()
        {
            if (AuthService.CurrentUser == null) return;
            MonthOptions = _transactionService.GetAvailableMonths(AuthService.CurrentUser.Id, _isOffice);

            var todayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var match = MonthOptions.FirstOrDefault(m => m.Year == todayMonth.Year && m.Month == todayMonth.Month);
            _selectedMonth = match != default ? match : (MonthOptions.FirstOrDefault() != default ? MonthOptions.FirstOrDefault() : todayMonth);
        }

        public void LoadReportsData()
        {
            if (AuthService.CurrentUser == null) return;

            int userId = AuthService.CurrentUser.Id;

            // 1. Monthly Summary from actual DB
            var summary = _transactionService.GetMonthlySummary(userId, SelectedMonth, _isOffice);
            Income = summary.TotalIncome;
            Expenses = summary.TotalExpenses;
            Balance = summary.Balance;

            ExpensesDelta = summary.ExpensesDelta;
            ExpensesChangePct = summary.ExpensesChangePercentage;
            IncomeDelta = summary.IncomeDelta;
            IncomeChangePct = summary.IncomeChangePercentage;

            // 2. Budget status from actual DB
            var statusReport = _transactionService.GetBudgetStatus(userId, SelectedMonth, _isOffice);
            BudgetLimit = statusReport.TargetAmount;
            Remaining = statusReport.RemainingAmount;
            RemainingPercentage = statusReport.RemainingPercentage;
            UsedPercentage = statusReport.PercentageUsed;
            BudgetHealth = statusReport.Health;
            BudgetStatusMessage = statusReport.StatusMessage;

            BudgetInputText = BudgetLimit > 0 ? BudgetLimit.ToString("F0") : string.Empty;

            // 3. Category spending breakdown
            var rawCategories = _transactionService.GetCategoryBreakdown(userId, SelectedMonth, _isOffice, "Expense");
            string[] palette = { "#007AFF", "#2ECC71", "#FF9800", "#9B59B6", "#F1C40F", "#E74C3C", "#1ABC9C", "#34495E", "#E67E22" };
            int i = 0;
            var list = new List<CategorySpendingItem>();
            foreach (var cat in rawCategories)
            {
                list.Add(new CategorySpendingItem
                {
                    Category = cat.Category,
                    Spent = cat.TotalAmount,
                    PercentageOfTotal = cat.Percentage,
                    ColorBrush = palette[i % palette.Length]
                });
                i++;
            }
            CategoryBreakdown = new ObservableCollection<CategorySpendingItem>(list);
            OnPropertyChanged(nameof(HasSpending));
        }

        private void ExecuteSaveBudget()
        {
            if (AuthService.CurrentUser == null) return;

            if (decimal.TryParse(BudgetInputText, out decimal newLimit) && newLimit >= 0)
            {
                string monthStr = SelectedMonth.ToString("yyyy-MM");
                var budget = new Budget
                {
                    UserId = AuthService.CurrentUser.Id,
                    Month = monthStr,
                    TargetAmount = newLimit,
                    IsOffice = _isOffice
                };

                _transactionService.SaveOrUpdateBudget(budget);
                LoadReportsData();
            }
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
    }
}
