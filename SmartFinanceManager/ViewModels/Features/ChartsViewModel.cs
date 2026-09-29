using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class CategoryPercentageItem
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string ColorBrush { get; set; } = "#007AFF";
        public string IconTag { get; set; } = "IconOthers";
    }

    public class ChartsViewModel : ViewModelBase
    {
        private readonly TransactionService _transactionService;
        private readonly bool _isOffice;

        private DateTime _selectedMonth = DateTime.Now;
        private List<DateTime> _monthOptions;
        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _totalTransfers;
        private decimal _netBalance;
        private decimal _totalCashFlow;
        private double _incomePercentage;
        private double _expensePercentage;
        private string _incomePercentageText = "0%";
        private string _expensePercentageText = "0%";
        private string _coverageRatioText = "No Activity";
        private string _coverageSubtext = "No financial records this month";
        private double _incomeBarHeight = 120.0;
        private double _expenseBarHeight = 14.0;
        private double _incomeVisualWidth = 250.0;
        private double _expenseVisualWidth = 250.0;
        private string _incomeSlicePathData = string.Empty;
        private string _expenseSlicePathData = string.Empty;
        private double _incomeLabelX = 110;
        private double _incomeLabelY = 120;
        private double _expenseLabelX = 110;
        private double _expenseLabelY = 120;
        private bool _showIncomeLabel;
        private bool _showExpenseLabel;
        private string _savingsRateLabel = string.Empty;
        private string _chartInsight = string.Empty;
        private ObservableCollection<CategoryPercentageItem> _expenseCategories;
        private ObservableCollection<CategoryPercentageItem> _incomeCategories;

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
                    LoadChartData();
                }
            }
        }

        public string SelectedMonthLabel => SelectedMonth.ToString("MMMM yyyy");

        public decimal TotalIncome
        {
            get => _totalIncome;
            set
            {
                if (SetProperty(ref _totalIncome, value))
                {
                    OnPropertyChanged(nameof(HasIncomeData));
                }
            }
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set
            {
                if (SetProperty(ref _totalExpenses, value))
                {
                    OnPropertyChanged(nameof(HasExpenseData));
                }
            }
        }

        public decimal TotalTransfers
        {
            get => _totalTransfers;
            set
            {
                if (SetProperty(ref _totalTransfers, value))
                {
                    OnPropertyChanged(nameof(HasTransfers));
                }
            }
        }

        public bool HasTransfers => TotalTransfers > 0;

        public decimal NetBalance
        {
            get => _netBalance;
            set
            {
                if (SetProperty(ref _netBalance, value))
                {
                    OnPropertyChanged(nameof(IsSurplus));
                    OnPropertyChanged(nameof(SurplusBadgeText));
                }
            }
        }

        public decimal TotalCashFlow
        {
            get => _totalCashFlow;
            set
            {
                if (SetProperty(ref _totalCashFlow, value))
                {
                    OnPropertyChanged(nameof(HasData));
                }
            }
        }

        public double IncomePercentage
        {
            get => _incomePercentage;
            set => SetProperty(ref _incomePercentage, value);
        }

        public double ExpensePercentage
        {
            get => _expensePercentage;
            set => SetProperty(ref _expensePercentage, value);
        }

        public string IncomePercentageText
        {
            get => _incomePercentageText;
            set => SetProperty(ref _incomePercentageText, value);
        }

        public string ExpensePercentageText
        {
            get => _expensePercentageText;
            set => SetProperty(ref _expensePercentageText, value);
        }

        public string CoverageRatioText
        {
            get => _coverageRatioText;
            set => SetProperty(ref _coverageRatioText, value);
        }

        public string CoverageSubtext
        {
            get => _coverageSubtext;
            set => SetProperty(ref _coverageSubtext, value);
        }

        public double IncomeBarHeight
        {
            get => _incomeBarHeight;
            set => SetProperty(ref _incomeBarHeight, value);
        }

        public double ExpenseBarHeight
        {
            get => _expenseBarHeight;
            set => SetProperty(ref _expenseBarHeight, value);
        }

        public double IncomeVisualWidth
        {
            get => _incomeVisualWidth;
            set => SetProperty(ref _incomeVisualWidth, value);
        }

        public double ExpenseVisualWidth
        {
            get => _expenseVisualWidth;
            set => SetProperty(ref _expenseVisualWidth, value);
        }

        public string IncomeSlicePathData
        {
            get => _incomeSlicePathData;
            set => SetProperty(ref _incomeSlicePathData, value);
        }

        public string ExpenseSlicePathData
        {
            get => _expenseSlicePathData;
            set => SetProperty(ref _expenseSlicePathData, value);
        }

        public double IncomeLabelX
        {
            get => _incomeLabelX;
            set => SetProperty(ref _incomeLabelX, value);
        }

        public double IncomeLabelY
        {
            get => _incomeLabelY;
            set => SetProperty(ref _incomeLabelY, value);
        }

        public double ExpenseLabelX
        {
            get => _expenseLabelX;
            set => SetProperty(ref _expenseLabelX, value);
        }

        public double ExpenseLabelY
        {
            get => _expenseLabelY;
            set => SetProperty(ref _expenseLabelY, value);
        }

        public bool ShowIncomeLabel
        {
            get => _showIncomeLabel;
            set => SetProperty(ref _showIncomeLabel, value);
        }

        public bool ShowExpenseLabel
        {
            get => _showExpenseLabel;
            set => SetProperty(ref _showExpenseLabel, value);
        }

        public bool HasIncomeData => TotalIncome > 0;
        public bool HasExpenseData => TotalExpenses > 0;
        public bool HasData => TotalCashFlow > 0;
        public bool IsSurplus => NetBalance >= 0;
        public string SurplusBadgeText => NetBalance >= 0 ? "SURPLUS" : "DEFICIT";

        public string SavingsRateLabel
        {
            get => _savingsRateLabel;
            set => SetProperty(ref _savingsRateLabel, value);
        }

        public string ChartInsight
        {
            get => _chartInsight;
            set => SetProperty(ref _chartInsight, value);
        }

        public ObservableCollection<CategoryPercentageItem> ExpenseCategories
        {
            get => _expenseCategories;
            set
            {
                if (SetProperty(ref _expenseCategories, value))
                {
                    OnPropertyChanged(nameof(HasExpenseCategories));
                }
            }
        }

        public ObservableCollection<CategoryPercentageItem> IncomeCategories
        {
            get => _incomeCategories;
            set
            {
                if (SetProperty(ref _incomeCategories, value))
                {
                    OnPropertyChanged(nameof(HasIncomeCategories));
                }
            }
        }

        public bool HasExpenseCategories => ExpenseCategories != null && ExpenseCategories.Count > 0;
        public bool HasIncomeCategories => IncomeCategories != null && IncomeCategories.Count > 0;

        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }

        public ChartsViewModel(bool isOffice) : this(null, isOffice)
        {
        }

        public ChartsViewModel(TransactionService? transactionService = null, bool isOffice = false)
        {
            _transactionService = transactionService ?? new TransactionService();
            _isOffice = isOffice;
            _expenseCategories = new ObservableCollection<CategoryPercentageItem>();
            _incomeCategories = new ObservableCollection<CategoryPercentageItem>();
            _monthOptions = new List<DateTime>();

            PreviousMonthCommand = new RelayCommand(ExecutePreviousMonth);
            NextMonthCommand = new RelayCommand(ExecuteNextMonth);

            InitializeMonths();
            LoadChartData();
        }

        private void InitializeMonths()
        {
            if (AuthService.CurrentUser == null) return;
            MonthOptions = _transactionService.GetAvailableMonths(AuthService.CurrentUser.Id, _isOffice);

            var todayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var match = MonthOptions.FirstOrDefault(m => m.Year == todayMonth.Year && m.Month == todayMonth.Month);
            _selectedMonth = match != default ? match : (MonthOptions.FirstOrDefault() != default ? MonthOptions.FirstOrDefault() : todayMonth);
        }

        public void LoadChartData()
        {
            if (AuthService.CurrentUser == null) return;

            int userId = AuthService.CurrentUser.Id;

            // 1. Calculate Monthly Summary
            var summary = _transactionService.GetMonthlySummary(userId, SelectedMonth, _isOffice);
            TotalIncome = summary.TotalIncome;
            TotalExpenses = summary.TotalExpenses;
            TotalTransfers = summary.TotalTransfers;
            NetBalance = summary.Balance;
            TotalCashFlow = TotalIncome + TotalExpenses;

            if (TotalCashFlow > 0)
            {
                double rawInc = (double)(TotalIncome / TotalCashFlow) * 100.0;
                double rawExp = (double)(TotalExpenses / TotalCashFlow) * 100.0;
                IncomePercentage = rawInc;
                ExpensePercentage = rawExp;

                // Formatted percentage strings that NEVER display 0% when money was spent!
                if (TotalExpenses == 0)
                {
                    IncomePercentageText = "100%";
                    ExpensePercentageText = "0%";
                    CoverageRatioText = "100% Retained";
                    CoverageSubtext = "Zero expenses recorded for this month";
                    IncomeBarHeight = 120.0;
                    ExpenseBarHeight = 0.0;
                    IncomeVisualWidth = 500.0;
                    ExpenseVisualWidth = 0.0;
                }
                else if (TotalIncome == 0)
                {
                    IncomePercentageText = "0%";
                    ExpensePercentageText = "100%";
                    CoverageRatioText = "100% Outflow";
                    CoverageSubtext = "Zero income recorded for this month";
                    IncomeBarHeight = 0.0;
                    ExpenseBarHeight = 120.0;
                    IncomeVisualWidth = 0.0;
                    ExpenseVisualWidth = 500.0;
                }
                else
                {
                    // Percentage texts
                    if (rawExp < 0.01)
                    {
                        ExpensePercentageText = "< 0.01%";
                        IncomePercentageText = "> 99.99%";
                    }
                    else if (rawExp < 0.1)
                    {
                        ExpensePercentageText = $"{rawExp:F2}%";
                        IncomePercentageText = $"{rawInc:F2}%";
                    }
                    else if (rawInc < 0.01)
                    {
                        IncomePercentageText = "< 0.01%";
                        ExpensePercentageText = "> 99.99%";
                    }
                    else if (rawInc < 0.1)
                    {
                        IncomePercentageText = $"{rawInc:F2}%";
                        ExpensePercentageText = $"{rawExp:F2}%";
                    }
                    else if (rawExp < 1.0 || rawInc < 1.0)
                    {
                        IncomePercentageText = $"{rawInc:F1}%";
                        ExpensePercentageText = $"{rawExp:F1}%";
                    }
                    else
                    {
                        int ir = (int)Math.Round(rawInc, MidpointRounding.AwayFromZero);
                        int er = 100 - ir;
                        if (ir == 0 && TotalIncome > 0) { ir = 1; er = 99; }
                        if (er == 0 && TotalExpenses > 0) { er = 1; ir = 99; }
                        IncomePercentageText = $"{ir}%";
                        ExpensePercentageText = $"{er}%";
                    }

                    // Comparative Ratio & Coverage metrics
                    if (TotalIncome >= TotalExpenses)
                    {
                        decimal ratio = Math.Round(TotalIncome / TotalExpenses, 1);
                        CoverageRatioText = FormattableString.Invariant($"{ratio:N1}× Inflow Coverage");
                        CoverageSubtext = FormattableString.Invariant($"Earned ${ratio:N2} for every $1 spent");
                        IncomeBarHeight = 120.0;
                        double ratioVal = (double)(TotalExpenses / TotalIncome);
                        ExpenseBarHeight = Math.Max(14.0, Math.Min(120.0, ratioVal * 120.0));
                    }
                    else
                    {
                        decimal ratio = Math.Round(TotalExpenses / TotalIncome, 1);
                        CoverageRatioText = FormattableString.Invariant($"{ratio:N1}× Spending Deficit");
                        CoverageSubtext = FormattableString.Invariant($"Spent ${ratio:N2} for every $1 earned");
                        ExpenseBarHeight = 120.0;
                        double ratioVal = (double)(TotalIncome / TotalExpenses);
                        IncomeBarHeight = Math.Max(14.0, Math.Min(120.0, ratioVal * 120.0));
                    }

                    // Widths for continuous live split meter (min 20px so small values are always clearly visible)
                    const double totalWidth = 500.0;
                    const double minW = 20.0;
                    double available = totalWidth - (2.0 * minW);
                    IncomeVisualWidth = minW + available * (rawInc / 100.0);
                    ExpenseVisualWidth = totalWidth - IncomeVisualWidth;
                }
            }
            else
            {
                IncomePercentage = 0;
                ExpensePercentage = 0;
                IncomePercentageText = "0%";
                ExpensePercentageText = "0%";
                CoverageRatioText = "No Activity";
                CoverageSubtext = "No financial records this month";
                IncomeBarHeight = 0.0;
                ExpenseBarHeight = 0.0;
                IncomeVisualWidth = 250.0;
                ExpenseVisualWidth = 250.0;
            }

            // Update Slices and Label Positioning
            UpdatePieChartGeometry();

            // Savings rate label
            if (TotalIncome > 0)
            {
                double savingsRate = (double)Math.Round(((TotalIncome - TotalExpenses) / TotalIncome) * 100m, 1);
                SavingsRateLabel = savingsRate >= 0 ? $"+{savingsRate:F1}% retained" : $"{savingsRate:F1}% deficit";
            }
            else if (TotalExpenses > 0)
            {
                SavingsRateLabel = "100% expenses";
            }
            else
            {
                SavingsRateLabel = "No activity";
            }

            // Category breakdown for backwards compatibility
            var rawExpenses = _transactionService.GetCategoryBreakdown(userId, SelectedMonth, _isOffice, "Expense");
            var expenseList = new List<CategoryPercentageItem>();
            string[] expenseColors = { "#FF3B30", "#FF6B6B", "#FF8E53", "#E74C3C", "#C0392B", "#D35400", "#E67E22" };
            int eIdx = 0;
            foreach (var item in rawExpenses)
            {
                expenseList.Add(new CategoryPercentageItem
                {
                    Category = item.Category,
                    Amount = item.TotalAmount,
                    Percentage = item.Percentage,
                    ColorBrush = expenseColors[eIdx % expenseColors.Length]
                });
                eIdx++;
            }
            ExpenseCategories = new ObservableCollection<CategoryPercentageItem>(expenseList);

            var rawIncome = _transactionService.GetCategoryBreakdown(userId, SelectedMonth, _isOffice, "Income");
            var incomeList = new List<CategoryPercentageItem>();
            string[] incomeColors = { "#007AFF", "#3498DB", "#2ECC71", "#1ABC9C", "#16A085" };
            int iIdx = 0;
            foreach (var item in rawIncome)
            {
                incomeList.Add(new CategoryPercentageItem
                {
                    Category = item.Category,
                    Amount = item.TotalAmount,
                    Percentage = item.Percentage,
                    ColorBrush = incomeColors[iIdx % incomeColors.Length]
                });
                iIdx++;
            }
            IncomeCategories = new ObservableCollection<CategoryPercentageItem>(incomeList);

            // Generate Insight
            if (TotalCashFlow == 0)
            {
                ChartInsight = $"No financial transactions recorded for {SelectedMonth:MMMM yyyy}.";
            }
            else if (TotalIncome >= TotalExpenses)
            {
                ChartInsight = $"Healthy Surplus: Earned ${TotalIncome:N2} vs ${TotalExpenses:N2} spent in {SelectedMonth:MMMM yyyy} ({SavingsRateLabel}).";
            }
            else
            {
                ChartInsight = $"Spending Alert: Expenses (${TotalExpenses:N2}) exceeded Income (${TotalIncome:N2}) by ${Math.Abs(NetBalance):N2} in {SelectedMonth:MMMM yyyy}.";
            }
        }

        private void UpdatePieChartGeometry()
        {
            if (TotalCashFlow <= 0)
            {
                IncomeSlicePathData = string.Empty;
                ExpenseSlicePathData = string.Empty;
                ShowIncomeLabel = false;
                ShowExpenseLabel = false;
                return;
            }

            const double cx = 130.0;
            const double cy = 130.0;
            const double r = 110.0;
            const double rLabel = 60.0;

            // Pure 100% Income (strictly 0 expense)
            if (TotalExpenses <= 0)
            {
                IncomeSlicePathData = FormattableString.Invariant(
                    $"M {cx:F1},{cy - r:F1} A {r:F1},{r:F1} 0 1,1 {cx:F1},{cy + r:F1} A {r:F1},{r:F1} 0 1,1 {cx:F1},{cy - r:F1} Z");
                ExpenseSlicePathData = string.Empty;
                IncomeLabelX = cx - 20;
                IncomeLabelY = cy - 10;
                ShowIncomeLabel = true;
                ShowExpenseLabel = false;
                return;
            }

            // Pure 100% Expense (strictly 0 income)
            if (TotalIncome <= 0)
            {
                IncomeSlicePathData = string.Empty;
                ExpenseSlicePathData = FormattableString.Invariant(
                    $"M {cx:F1},{cy - r:F1} A {r:F1},{r:F1} 0 1,1 {cx:F1},{cy + r:F1} A {r:F1},{r:F1} 0 1,1 {cx:F1},{cy - r:F1} Z");
                ExpenseLabelX = cx - 20;
                ExpenseLabelY = cy - 10;
                ShowIncomeLabel = false;
                ShowExpenseLabel = true;
                return;
            }

            // Both exist: Ensure a minimum visible wedge angle (12.0 deg) so even tiny fractions are never invisible!
            const double minWedgeDeg = 14.0;
            double degInc = (IncomePercentage / 100.0) * 360.0;
            double degExp = (ExpensePercentage / 100.0) * 360.0;

            if (degExp < minWedgeDeg)
            {
                degExp = minWedgeDeg;
                degInc = 360.0 - minWedgeDeg;
            }
            else if (degInc < minWedgeDeg)
            {
                degInc = minWedgeDeg;
                degExp = 360.0 - minWedgeDeg;
            }

            // Top point (12 o'clock, angle = -90 deg)
            double x0 = cx;
            double y0 = cy - r;

            // Divider point between Income (clockwise from top) and Expense
            double angle1Deg = -90.0 + degInc;
            double rad1 = angle1Deg * (Math.PI / 180.0);
            double x1 = cx + r * Math.Cos(rad1);
            double y1 = cy + r * Math.Sin(rad1);

            int largeArcInc = degInc > 180.0 ? 1 : 0;
            int largeArcExp = degExp > 180.0 ? 1 : 0;

            IncomeSlicePathData = FormattableString.Invariant(
                $"M {cx:F1},{cy:F1} L {x0:F1},{y0:F1} A {r:F1},{r:F1} 0 {largeArcInc},1 {x1:F1},{y1:F1} Z");

            ExpenseSlicePathData = FormattableString.Invariant(
                $"M {cx:F1},{cy:F1} L {x1:F1},{y1:F1} A {r:F1},{r:F1} 0 {largeArcExp},1 {x0:F1},{y0:F1} Z");

            // Percentage Label positions
            double midAngleInc = (-90.0 + degInc / 2.0) * (Math.PI / 180.0);
            IncomeLabelX = cx + rLabel * Math.Cos(midAngleInc) - 20;
            IncomeLabelY = cy + rLabel * Math.Sin(midAngleInc) - 10;
            ShowIncomeLabel = true;

            double midAngleExp = (-90.0 + degInc + degExp / 2.0) * (Math.PI / 180.0);
            if (degExp < 25.0)
            {
                // Place just slightly outside or near the wedge edge so text doesn't squeeze
                ExpenseLabelX = cx + (r * 0.72) * Math.Cos(midAngleExp) - 20;
                ExpenseLabelY = cy + (r * 0.72) * Math.Sin(midAngleExp) - 10;
            }
            else
            {
                ExpenseLabelX = cx + rLabel * Math.Cos(midAngleExp) - 20;
                ExpenseLabelY = cy + rLabel * Math.Sin(midAngleExp) - 10;
            }
            ShowExpenseLabel = true;
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
