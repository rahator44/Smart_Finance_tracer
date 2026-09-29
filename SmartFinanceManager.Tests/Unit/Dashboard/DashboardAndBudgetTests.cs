using System;
using System.Linq;
using Xunit;
using SmartFinanceManager.Services;
using SmartFinanceManager.Data;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Tests
{
    public class DashboardAndBudgetTests
    {
        private readonly TransactionService _transactionService;
        private readonly AuthService _authService;

        public DashboardAndBudgetTests()
        {
            _transactionService = new TransactionService();
            _authService = new AuthService();
            using var context = new FinanceDbContext();
            context.Database.EnsureCreated();
        }

        [Fact]
        public void PersonalDashboardCalculations_AreExact()
        {
            string email = $"personal_math_{Guid.NewGuid():N}@finance.com";
            _authService.Register("Math User", email, "password123", "Personal", out _);
            _authService.Login(email, "password123", false, out _);
            int userId = AuthService.CurrentUser!.Id;

            var month = new DateTime(2026, 9, 1);

            // Income: 3000 + 500 = 3500
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Income", Amount = 3000, Date = month, IsOffice = false });
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Income", Amount = 500, Date = month, IsOffice = false });

            // Expenses: 400 + 600 = 1000
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -400, Date = month, IsOffice = false });
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -600, Date = month, IsOffice = false });

            var summary = _transactionService.GetMonthlySummary(userId, month, isOffice: false);

            Assert.Equal(3500, summary.TotalIncome);
            Assert.Equal(1000, summary.TotalExpenses);
            Assert.Equal(2500, summary.Balance); // 3500 - 1000
        }

        [Fact]
        public void OfficeDashboardCalculations_RevenueProfitAndCashFlow_AreExact()
        {
            string email = $"office_math_{Guid.NewGuid():N}@finance.com";
            _authService.Register("Office Corp", email, "password123", "Office", out _);
            _authService.Login(email, "password123", false, out _);
            int userId = AuthService.CurrentUser!.Id;

            var month = new DateTime(2026, 9, 1);

            // Revenue: 20000
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Income", Amount = 20000, Date = month, IsOffice = true });

            // Expenses: 8000 + 3000 = 11000
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -8000, Date = month, IsOffice = true });
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -3000, Date = month, IsOffice = true });

            // Transfer: +2500 (Capital Injection)
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Transfer", Amount = 2500, Date = month, IsOffice = true });

            var summary = _transactionService.GetMonthlySummary(userId, month, isOffice: true);

            Assert.Equal(20000, summary.TotalRevenue);
            Assert.Equal(11000, summary.TotalExpenses);
            Assert.Equal(9000, summary.NetProfit); // 20000 - 11000
            Assert.Equal(2500, summary.NetTransfers);
            Assert.Equal(11500, summary.CashFlow); // 9000 + 2500
        }

        [Fact]
        public void BudgetHealth_Thresholds_EvaluatedProperly()
        {
            string email = $"budget_user_{Guid.NewGuid():N}@finance.com";
            _authService.Register("Budget User", email, "password123", "Personal", out _);
            _authService.Login(email, "password123", false, out _);
            int userId = AuthService.CurrentUser!.Id;

            var month = new DateTime(2026, 9, 1);
            string monthStr = "2026-09";

            // Set Budget: $1000
            _transactionService.SaveOrUpdateBudget(new Budget
            {
                UserId = userId,
                Month = monthStr,
                TargetAmount = 1000,
                IsOffice = false
            });

            // 1. Spend $500 (50%) -> Healthy
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -500, Date = month, IsOffice = false });
            var status1 = _transactionService.GetBudgetStatus(userId, month, isOffice: false);
            Assert.Equal(BudgetHealth.Healthy, status1.Health);
            Assert.Equal(500, status1.RemainingAmount);
            Assert.Equal(50, status1.PercentageUsed);

            // 2. Spend another $350 (Total: $850 = 85%) -> Warning
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -350, Date = month, IsOffice = false });
            var status2 = _transactionService.GetBudgetStatus(userId, month, isOffice: false);
            Assert.Equal(BudgetHealth.Warning, status2.Health);
            Assert.Equal(150, status2.RemainingAmount);
            Assert.Equal(85, status2.PercentageUsed);

            // 3. Spend another $200 (Total: $1050 = 105%) -> Exceeded
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -200, Date = month, IsOffice = false });
            var status3 = _transactionService.GetBudgetStatus(userId, month, isOffice: false);
            Assert.Equal(BudgetHealth.Exceeded, status3.Health);
            Assert.Equal(-50, status3.RemainingAmount);
            Assert.Contains("Budget Exceeded", status3.StatusMessage);
        }

        [Fact]
        public void SmartCategorizationService_AccuratelyIdentifiesKeywords()
        {
            // Personal predictions
            var r1 = SmartCategorizationService.Predict("Morning Starbucks coffee and breakfast", isOffice: false);
            Assert.True(r1.IsConfident);
            Assert.Equal("Expense", r1.Type);
            Assert.Equal("Food & Dining", r1.Category);

            var r2 = SmartCategorizationService.Predict("Uber ride to international terminal", isOffice: false);
            Assert.True(r2.IsConfident);
            Assert.Equal("Expense", r2.Type);
            Assert.Equal("Transportation", r2.Category);

            var r3 = SmartCategorizationService.Predict("Monthly job salary direct deposit", isOffice: false);
            Assert.True(r3.IsConfident);
            Assert.Equal("Income", r3.Type);
            Assert.Equal("Salary & Wages", r3.Category);

            var r4 = SmartCategorizationService.Predict("Monthly apartment rent to landlord", isOffice: false);
            Assert.True(r4.IsConfident);
            Assert.Equal("Expense", r4.Type);
            Assert.Equal("Housing", r4.Category);

            // Office predictions
            var o1 = SmartCategorizationService.Predict("AWS cloud infrastructure and hosting", isOffice: true);
            Assert.True(o1.IsConfident);
            Assert.Equal("Expense", o1.Type);
            Assert.Equal("Software & SaaS", o1.Category);

            var o2 = SmartCategorizationService.Predict("Bi-weekly team payroll and compensation", isOffice: true);
            Assert.True(o2.IsConfident);
            Assert.Equal("Expense", o2.Type);
            Assert.Equal("Staff Salaries & Payroll", o2.Category);

            var o3 = SmartCategorizationService.Predict("Q3 Enterprise customer license sales", isOffice: true);
            Assert.True(o3.IsConfident);
            Assert.Equal("Income", o3.Type);
            Assert.Equal("Client Sales", o3.Category);

            var o4 = SmartCategorizationService.Predict("Operating reserve treasury wire", isOffice: true);
            Assert.True(o4.IsConfident);
            Assert.Equal("Transfer", o4.Type);
            Assert.Equal("Treasury", o4.Category);
        }

        [Fact]
        public void TwoDataMonthlySummary_CalculatesExactIncomeAndExpenseDistribution()
        {
            string email = $"piechart_{Guid.NewGuid():N}@finance.com";
            _authService.Register("Pie User", email, "password123", "Personal", out _);
            _authService.Login(email, "password123", false, out _);
            int userId = AuthService.CurrentUser!.Id;

            var month = new DateTime(2026, 9, 1);

            // Income: 6000 (60%)
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Income", Amount = 6000, Date = month, IsOffice = false });

            // Expenses: 4000 (40%)
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -4000, Date = month, IsOffice = false });

            var summary = _transactionService.GetMonthlySummary(userId, month, isOffice: false);

            Assert.Equal(6000, summary.TotalIncome);
            Assert.Equal(4000, summary.TotalExpenses);
            decimal totalCashFlow = summary.TotalIncome + summary.TotalExpenses;
            Assert.Equal(10000, totalCashFlow);

            double incPct = (double)(summary.TotalIncome / totalCashFlow) * 100.0;
            double expPct = 100.0 - incPct;

            Assert.Equal(60.0, incPct);
            Assert.Equal(40.0, expPct);
        }
        [Fact]
        public void TwoDataMonthlySummary_ExtremeRatio_PreservesBothIncomeAndExpenseVisibility()
        {
            string email = $"extreme_{Guid.NewGuid():N}@finance.com";
            _authService.Register("Extreme User", email, "password123", "Personal", out _);
            _authService.Login(email, "password123", false, out _);
            int userId = AuthService.CurrentUser!.Id;

            var month = new DateTime(2026, 9, 1);

            // $50,000,000 Income vs $6,500 Expense (as tested by user)
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Income", Amount = 50000000, Date = month, IsOffice = false });
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -6500, Date = month, IsOffice = false });

            var summary = _transactionService.GetMonthlySummary(userId, month, isOffice: false);

            Assert.Equal(50000000, summary.TotalIncome);
            Assert.Equal(6500, summary.TotalExpenses);
            Assert.Equal(49993500, summary.Balance);

            decimal totalCashFlow = summary.TotalIncome + summary.TotalExpenses;
            double incPct = (double)(summary.TotalIncome / totalCashFlow) * 100.0;
            double expPct = (double)(summary.TotalExpenses / totalCashFlow) * 100.0;

            Assert.True(expPct > 0.0);
            Assert.True(incPct > 99.0);
        }
        [Fact]
        public void PersonalTransfers_PersistToDatabase_AndDeductFromLiquidBalance_AndReflectInSummary()
        {
            string email = $"transfer_user_{Guid.NewGuid():N}@finance.com";
            _authService.Register("Transfer User", email, "password123", "Personal", out _);
            _authService.Login(email, "password123", false, out _);
            int userId = AuthService.CurrentUser!.Id;

            var month = new DateTime(2026, 9, 1);

            // 1. Income: ,000
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Income", Amount = 5000, Date = month, IsOffice = false });
            // 2. Expenses: ,200
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Expense", Amount = -1200, Date = month, IsOffice = false });
            // 3. Transfers: ,000 to Investments + 00 to Savings Account
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Transfer", Category = "Investments", Amount = -1000, Date = month, IsOffice = false });
            _transactionService.AddTransaction(new Transaction { UserId = userId, Type = "Transfer", Category = "Savings Account", Amount = -500, Date = month, IsOffice = false });

            // Verify SQLite DB persistence via GetFilteredTransactions
            var transfers = _transactionService.GetFilteredTransactions(userId, month, isOffice: false, typeFilter: "Transfer");
            Assert.Equal(2, transfers.Count);
            Assert.Contains(transfers, t => t.Category == "Investments");
            Assert.Contains(transfers, t => t.Category == "Savings Account");

            // Verify MonthlySummary calculation
            var summary = _transactionService.GetMonthlySummary(userId, month, isOffice: false);
            Assert.Equal(5000, summary.TotalIncome);
            Assert.Equal(1200, summary.TotalExpenses);
            Assert.Equal(1500, summary.TotalTransfers);
            // Liquid Balance = Income (5000) - Expenses (1200) - Transfers (1500) = 2300
            Assert.Equal(2300, summary.Balance);
        }
    }
}
