// [Rahat Enterprise Logic] B2B transaction handling with corporate tax and client sales categorization
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartFinanceManager.Data;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Services
{
    public class MonthlySummary
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalRevenue => TotalIncome;
        public decimal NetProfit => TotalIncome - TotalExpenses;
        public decimal TotalTransfers { get; set; }
        public decimal NetTransfers { get; set; }
        public decimal CashFlow => NetProfit + NetTransfers;
        public int TransactionCount { get; set; }

        // Comparison deltas against previous month
        public decimal ExpensesDelta { get; set; }
        public double ExpensesChangePercentage { get; set; }
        public decimal IncomeDelta { get; set; }
        public double IncomeChangePercentage { get; set; }
    }

    public class CategoryGroupSummary
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
        public int Count { get; set; }
    }

    public enum BudgetHealth
    {
        Healthy,
        Warning,  // >= 80% used
        Exceeded  // >= 100% used
    }

    public class BudgetStatusReport
    {
        public decimal TargetAmount { get; set; }
        public decimal ActualSpent { get; set; }
        public decimal RemainingAmount { get; set; }
        public double PercentageUsed { get; set; }
        public double RemainingPercentage { get; set; }
        public BudgetHealth Health { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }

    public class TransactionService
    {
        public List<Transaction> GetFilteredTransactions(
            int userId, 
            DateTime month, 
            bool isOffice, 
            string? typeFilter = null, 
            string? categoryFilter = null, 
            string? searchTerm = null)
        {
            using (var context = new FinanceDbContext())
            {
                var startDate = new DateTime(month.Year, month.Month, 1);
                var endDate = startDate.AddMonths(1);

                var query = context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.IsOffice == isOffice && t.Date >= startDate && t.Date < endDate);

                // Filter by Type (Expense, Income, Transfer)
                if (!string.IsNullOrWhiteSpace(typeFilter) && !typeFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.Type.ToLower() == typeFilter.ToLower());
                }

                // Filter by Category
                if (!string.IsNullOrWhiteSpace(categoryFilter) && !categoryFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.Category.ToLower() == categoryFilter.ToLower());
                }

                // Filter by Search Keyword in Description or Category
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    string term = searchTerm.Trim().ToLower();
                    query = query.Where(t => t.Description.ToLower().Contains(term) || t.Category.ToLower().Contains(term));
                }

                return query.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).ToList();
            }
        }

        public List<Transaction> GetTransactions(int userId, DateTime month, bool isOffice)
        {
            return GetFilteredTransactions(userId, month, isOffice, null, null, null);
        }

        public List<Transaction> GetRecentTransactions(int userId, bool isOffice, int count = 5)
        {
            using (var context = new FinanceDbContext())
            {
                return context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.IsOffice == isOffice)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .Take(count)
                    .ToList();
            }
        }

        public MonthlySummary GetMonthlySummary(int userId, DateTime month, bool isOffice)
        {
            using (var context = new FinanceDbContext())
            {
                var startDate = new DateTime(month.Year, month.Month, 1);
                var endDate = startDate.AddMonths(1);

                var currentTxs = context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.IsOffice == isOffice && t.Date >= startDate && t.Date < endDate)
                    .ToList();

                decimal income = currentTxs.Where(t => t.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)).Sum(t => Math.Abs(t.Amount));
                decimal expenses = currentTxs.Where(t => t.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase)).Sum(t => Math.Abs(t.Amount));
                decimal transfers = currentTxs.Where(t => t.Type.Equals("Transfer", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
                decimal totalTransfers = currentTxs.Where(t => t.Type.Equals("Transfer", StringComparison.OrdinalIgnoreCase)).Sum(t => Math.Abs(t.Amount));

                var summary = new MonthlySummary
                {
                    TotalIncome = income,
                    TotalExpenses = expenses,
                    TotalTransfers = totalTransfers,
                    NetTransfers = transfers,
                    Balance = isOffice ? (income - expenses + transfers) : (income - expenses - totalTransfers),
                    TransactionCount = currentTxs.Count
                };

                // Calculate Previous Month deltas
                var prevStart = startDate.AddMonths(-1);
                var prevEnd = startDate.AddTicks(-1);

                var prevTxs = context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.IsOffice == isOffice && t.Date >= prevStart && t.Date <= prevEnd)
                    .ToList();

                decimal prevIncome = prevTxs.Where(t => t.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)).Sum(t => Math.Abs(t.Amount));
                decimal prevExpenses = prevTxs.Where(t => t.Type.Equals("Expense", StringComparison.OrdinalIgnoreCase)).Sum(t => Math.Abs(t.Amount));

                summary.ExpensesDelta = expenses - prevExpenses;
                summary.ExpensesChangePercentage = prevExpenses > 0 ? (double)((summary.ExpensesDelta / prevExpenses) * 100m) : 0;

                summary.IncomeDelta = income - prevIncome;
                summary.IncomeChangePercentage = prevIncome > 0 ? (double)((summary.IncomeDelta / prevIncome) * 100m) : 0;

                return summary;
            }
        }

        public List<CategoryGroupSummary> GetCategoryBreakdown(int userId, DateTime month, bool isOffice, string type = "Expense")
        {
            using (var context = new FinanceDbContext())
            {
                var startDate = new DateTime(month.Year, month.Month, 1);
                var endDate = startDate.AddMonths(1);

                var filtered = context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.IsOffice == isOffice && t.Type.ToLower() == type.ToLower() && t.Date >= startDate && t.Date < endDate)
                    .ToList();

                decimal total = filtered.Sum(t => Math.Abs(t.Amount));

                if (total == 0) return new List<CategoryGroupSummary>();

                return filtered
                    .GroupBy(t => t.Category)
                    .Select(g =>
                    {
                        decimal catSum = g.Sum(t => Math.Abs(t.Amount));
                        return new CategoryGroupSummary
                        {
                            Category = g.Key,
                            TotalAmount = catSum,
                            Percentage = (double)Math.Round((catSum / total) * 100m, 1),
                            Count = g.Count()
                        };
                    })
                    .OrderByDescending(c => c.TotalAmount)
                    .ToList();
            }
        }

        public List<DateTime> GetAvailableMonths(int userId, bool isOffice)
        {
            using (var context = new FinanceDbContext())
            {
                var monthsSet = new HashSet<string>();
                var results = new List<DateTime>();

                // 1. Add current month and past 11 months by default
                var baseDate = new DateTime(2026, 8, 1);
                var todayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                
                // Add today's month first
                monthsSet.Add(todayMonth.ToString("yyyy-MM"));
                results.Add(todayMonth);

                for (int i = 0; i < 12; i++)
                {
                    var m = baseDate.AddMonths(-i);
                    string key = m.ToString("yyyy-MM");
                    if (monthsSet.Add(key))
                    {
                        results.Add(m);
                    }
                }

                // 2. Query any other months from actual transactions
                var dbDates = context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.IsOffice == isOffice)
                    .Select(t => t.Date)
                    .ToList();

                foreach (var d in dbDates)
                {
                    var m = new DateTime(d.Year, d.Month, 1);
                    string key = m.ToString("yyyy-MM");
                    if (monthsSet.Add(key))
                    {
                        results.Add(m);
                    }
                }

                return results.OrderByDescending(d => d).ToList();
            }
        }

        public void AddTransaction(Transaction tx)
        {
            using (var context = new FinanceDbContext())
            {
                context.Transactions.Add(tx);
                context.SaveChanges();
            }
        }

        public bool UpdateTransaction(Transaction tx, int userId)
        {
            using (var context = new FinanceDbContext())
            {
                // Strict user isolation check
                var existing = context.Transactions.FirstOrDefault(t => t.Id == tx.Id && t.UserId == userId);
                if (existing != null)
                {
                    existing.Type = tx.Type;
                    existing.Category = tx.Category;
                    existing.Amount = tx.Amount;
                    existing.Date = tx.Date;
                    existing.Description = tx.Description;
                    existing.IsOffice = tx.IsOffice;
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public bool DeleteTransaction(int txId, int userId)
        {
            using (var context = new FinanceDbContext())
            {
                // Strict user isolation check
                var tx = context.Transactions.FirstOrDefault(t => t.Id == txId && t.UserId == userId);
                if (tx != null)
                {
                    context.Transactions.Remove(tx);
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public Budget GetBudget(int userId, string monthStr, bool isOffice)
        {
            using (var context = new FinanceDbContext())
            {
                var budget = context.Budgets
                    .AsNoTracking()
                    .FirstOrDefault(b => b.UserId == userId && b.Month == monthStr && b.IsOffice == isOffice);

                if (budget == null)
                {
                    budget = new Budget
                    {
                        UserId = userId,
                        Month = monthStr,
                        TargetAmount = 0,
                        IsOffice = isOffice
                    };
                }
                return budget;
            }
        }

        public void SaveOrUpdateBudget(Budget budget)
        {
            using (var context = new FinanceDbContext())
            {
                var match = context.Budgets
                    .FirstOrDefault(b => b.UserId == budget.UserId && b.Month == budget.Month && b.IsOffice == budget.IsOffice);

                if (match != null)
                {
                    match.TargetAmount = budget.TargetAmount;
                }
                else
                {
                    context.Budgets.Add(new Budget
                    {
                        UserId = budget.UserId,
                        Month = budget.Month,
                        TargetAmount = budget.TargetAmount,
                        IsOffice = budget.IsOffice
                    });
                }
                context.SaveChanges();
            }
        }

        public BudgetStatusReport GetBudgetStatus(int userId, DateTime month, bool isOffice)
        {
            string monthStr = month.ToString("yyyy-MM");
            var budget = GetBudget(userId, monthStr, isOffice);
            var summary = GetMonthlySummary(userId, month, isOffice);

            decimal target = budget.TargetAmount;
            decimal spent = summary.TotalExpenses;
            decimal remaining = target - spent;

            double pctUsed = target > 0 ? (double)Math.Round((spent / target) * 100m, 1) : 0;
            double pctLeft = target > 0 ? Math.Max(0, 100.0 - pctUsed) : 0;

            BudgetHealth health;
            string message;

            if (target <= 0)
            {
                health = BudgetHealth.Healthy;
                message = "No budget target set for this month. Set one below to track spending.";
            }
            else if (spent >= target)
            {
                health = BudgetHealth.Exceeded;
                message = $"🚨 Budget Exceeded! You have spent {pctUsed:F0}% of budget (${Math.Abs(remaining):N0} over limit).";
            }
            else if (pctUsed >= 80)
            {
                health = BudgetHealth.Warning;
                message = $"⚠️ Caution: Approaching budget limit ({pctUsed:F0}% used, only ${remaining:N0} remaining).";
            }
            else
            {
                health = BudgetHealth.Healthy;
                message = $"✓ Healthy spending: {pctLeft:F0}% (${remaining:N0}) remaining of your monthly budget.";
            }

            return new BudgetStatusReport
            {
                TargetAmount = target,
                ActualSpent = spent,
                RemainingAmount = remaining,
                PercentageUsed = pctUsed,
                RemainingPercentage = pctLeft,
                Health = health,
                StatusMessage = message
            };
        }
    }
}
