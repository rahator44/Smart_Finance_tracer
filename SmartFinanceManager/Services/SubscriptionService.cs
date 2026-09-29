using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartFinanceManager.Data;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Services
{
    public class AdminOverviewStatistics
    {
        public int TotalUsers { get; set; }
        public int PersonalUsers { get; set; }
        public int OfficeUsers { get; set; }

        public int TotalTransactions { get; set; }
        public decimal TotalTransactionVolume { get; set; }

        public int PersonalTransactions { get; set; }
        public decimal PersonalTotalIncome { get; set; }
        public decimal PersonalTotalExpenses { get; set; }

        public int OfficeTransactions { get; set; }
        public decimal OfficeTotalIncome { get; set; }
        public decimal OfficeTotalExpenses { get; set; }

        public int ActiveOfficeSubscribers { get; set; }
        public int PendingSubscriptions { get; set; }
        public int RejectedSubscriptions { get; set; }
        public decimal TotalSubscriptionRevenue { get; set; }

        // Site Financial Overview (Subscriptions = Income, Site Operating Expenses)
        public decimal SiteTotalIncome { get; set; }
        public decimal SiteTotalExpenses { get; set; }
        public decimal SiteNetProfit => SiteTotalIncome - SiteTotalExpenses;
        public List<Transaction> SiteExpenses { get; set; } = new();
    }

    public class SubscriptionService
    {
        public (bool Success, string Message, OfficeSubscription? Subscription) CreateSubscriptionRequest(
            int userId, 
            string email, 
            decimal amount, 
            string transactionId, 
            string paymentMethod = "bKash")
        {
            if (userId <= 0)
            {
                return (false, "Authentication required. Please log in first.", null);
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Email address is required.", null);
            }

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return (false, "Transaction ID is required. Please enter the bKash transaction ID.", null);
            }

            string cleanTxId = transactionId.Trim().ToUpperInvariant();
            if (cleanTxId.Length < 4)
            {
                return (false, "Invalid Transaction ID. Please enter a valid bKash Transaction ID.", null);
            }

            if (amount <= 0 || amount != AdminConfig.OfficeSubscriptionPrice)
            {
                return (false, $"Invalid subscription fee amount. Required amount is {AdminConfig.OfficeSubscriptionPrice:N0} BDT.", null);
            }

            using (var context = new FinanceDbContext())
            {
                var user = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    return (false, "User account not found.", null);
                }

                // Check if user already has an approved subscription
                var approvedSub = context.Subscriptions
                    .AsNoTracking()
                    .FirstOrDefault(s => s.UserId == userId && s.Status == SubscriptionStatus.Approved);

                if (approvedSub != null)
                {
                    return (false, "Your account already has an active Approved Office Finance subscription.", approvedSub);
                }

                // Check if user has an existing pending subscription
                var pendingSub = context.Subscriptions
                    .AsNoTracking()
                    .FirstOrDefault(s => s.UserId == userId && s.Status == SubscriptionStatus.Pending);

                if (pendingSub != null)
                {
                    return (false, "You already have a pending verification request under review. Please wait for administrator approval.", pendingSub);
                }

                // Check if another user previously used the exact same Transaction ID
                var duplicateTx = context.Subscriptions
                    .AsNoTracking()
                    .FirstOrDefault(s => s.TransactionId == cleanTxId && s.UserId != userId && s.Status != SubscriptionStatus.Rejected);

                if (duplicateTx != null)
                {
                    return (false, "This Transaction ID has already been submitted by another account. Please contact support if you believe this is an error.", null);
                }

                var subscription = new OfficeSubscription
                {
                    UserId = userId,
                    Email = email.Trim().ToLowerInvariant(),
                    Amount = amount,
                    TransactionId = cleanTxId,
                    PaymentMethod = paymentMethod,
                    Status = SubscriptionStatus.Pending,
                    SubmittedDate = DateTime.UtcNow
                };

                context.Subscriptions.Add(subscription);
                context.SaveChanges();

                return (true, "Verification submitted. Your subscription is pending administrator approval.", subscription);
            }
        }

        public SubscriptionStatus? GetSubscriptionStatus(int userId)
        {
            using (var context = new FinanceDbContext())
            {
                var sub = context.Subscriptions
                    .AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SubmittedDate)
                    .FirstOrDefault();

                return sub?.Status;
            }
        }

        public OfficeSubscription? GetLatestUserSubscription(int userId)
        {
            using (var context = new FinanceDbContext())
            {
                return context.Subscriptions
                    .AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SubmittedDate)
                    .FirstOrDefault();
            }
        }

        public bool HasActiveOfficeSubscription(int userId)
        {
            using (var context = new FinanceDbContext())
            {
                return context.Subscriptions
                    .AsNoTracking()
                    .Any(s => s.UserId == userId && s.Status == SubscriptionStatus.Approved);
            }
        }

        public bool HasOfficeAccess(User? user)
        {
            if (user == null) return false;

            // Admin bypasses subscription requirements completely
            if (AdminConfig.IsAdmin(user))
            {
                return true;
            }

            return HasActiveOfficeSubscription(user.Id);
        }

        public List<OfficeSubscription> GetAllSubscriptions()
        {
            using (var context = new FinanceDbContext())
            {
                return context.Subscriptions
                    .Include(s => s.User)
                    .AsNoTracking()
                    .OrderByDescending(s => s.SubmittedDate)
                    .ToList();
            }
        }

        public List<OfficeSubscription> GetPendingSubscriptions()
        {
            using (var context = new FinanceDbContext())
            {
                return context.Subscriptions
                    .Include(s => s.User)
                    .AsNoTracking()
                    .Where(s => s.Status == SubscriptionStatus.Pending)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ToList();
            }
        }

        public bool ApproveSubscription(int subscriptionId, int adminUserId, string? note, out string message)
        {
            using (var context = new FinanceDbContext())
            {
                var adminUser = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == adminUserId);
                if (adminUser == null || !AdminConfig.IsAdmin(adminUser))
                {
                    message = "Unauthorized: Only administrators can approve subscriptions.";
                    return false;
                }

                var sub = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
                if (sub == null)
                {
                    message = "Subscription record not found.";
                    return false;
                }

                sub.Status = SubscriptionStatus.Approved;
                sub.ReviewedDate = DateTime.UtcNow;
                sub.ApprovedDate = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(note))
                {
                    sub.AdminNote = note.Trim();
                }

                context.SaveChanges();
                message = "Subscription approved successfully. Office Finance access granted.";
                return true;
            }
        }

        public bool RejectSubscription(int subscriptionId, int adminUserId, string? note, out string message)
        {
            using (var context = new FinanceDbContext())
            {
                var adminUser = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == adminUserId);
                if (adminUser == null || !AdminConfig.IsAdmin(adminUser))
                {
                    message = "Unauthorized: Only administrators can reject subscriptions.";
                    return false;
                }

                var sub = context.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
                if (sub == null)
                {
                    message = "Subscription record not found.";
                    return false;
                }

                sub.Status = SubscriptionStatus.Rejected;
                sub.ReviewedDate = DateTime.UtcNow;
                sub.RejectedDate = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(note))
                {
                    sub.AdminNote = note.Trim();
                }

                context.SaveChanges();
                message = "Subscription rejected.";
                return true;
            }
        }

        public List<User> GetAllUsers()
        {
            using (var context = new FinanceDbContext())
            {
                return context.Users
                    .AsNoTracking()
                    .OrderByDescending(u => u.CreatedDate)
                    .ToList();
            }
        }

        public (bool Success, string Message, Transaction? Transaction) AddSiteOperatingExpense(
            int adminUserId,
            decimal amount,
            string category,
            string description,
            DateTime date)
        {
            if (amount <= 0)
            {
                return (false, "Please enter a valid expense amount greater than 0.", null);
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                category = "Operations & Maintenance";
            }

            using (var context = new FinanceDbContext())
            {
                var adminUser = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == adminUserId);
                if (adminUser == null || !AdminConfig.IsAdmin(adminUser))
                {
                    return (false, "Unauthorized: Only administrators can record site operating expenses.", null);
                }

                var tx = new Transaction
                {
                    UserId = adminUserId,
                    Type = "Expense",
                    Category = category.Trim(),
                    Amount = -Math.Abs(amount),
                    Date = date,
                    IsOffice = true,
                    Description = string.IsNullOrWhiteSpace(description) ? "Platform Operating Expense" : description.Trim()
                };

                context.Transactions.Add(tx);
                context.SaveChanges();

                return (true, "Site operating expense recorded successfully.", tx);
            }
        }

        public bool DeleteSiteOperatingExpense(int transactionId, int adminUserId)
        {
            using (var context = new FinanceDbContext())
            {
                var adminUser = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == adminUserId);
                if (adminUser == null || !AdminConfig.IsAdmin(adminUser))
                {
                    return false;
                }

                var tx = context.Transactions.FirstOrDefault(t => t.Id == transactionId && t.UserId == adminUserId && t.Type == "Expense");
                if (tx != null)
                {
                    context.Transactions.Remove(tx);
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public List<Transaction> GetSiteOperatingExpenses(int adminUserId)
        {
            using (var context = new FinanceDbContext())
            {
                return context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == adminUserId && t.Type == "Expense")
                    .OrderByDescending(t => t.Date)
                    .ToList();
            }
        }

        public AdminOverviewStatistics GetAdminOverviewStatistics()
        {
            using (var context = new FinanceDbContext())
            {
                var users = context.Users.AsNoTracking().ToList();
                var txs = context.Transactions.AsNoTracking().ToList();
                var subs = context.Subscriptions.AsNoTracking().ToList();

                var adminUser = users.FirstOrDefault(u => AdminConfig.IsAdmin(u));
                int adminId = adminUser?.Id ?? 0;

                var personalTxs = txs.Where(t => !t.IsOffice).ToList();
                var officeTxs = txs.Where(t => t.IsOffice && t.UserId != adminId).ToList();

                decimal pIncome = personalTxs.Where(t => t.Type == "Income").Sum(t => Math.Abs(t.Amount));
                decimal pExpense = personalTxs.Where(t => t.Type == "Expense").Sum(t => Math.Abs(t.Amount));

                decimal oIncome = officeTxs.Where(t => t.Type == "Income").Sum(t => Math.Abs(t.Amount));
                decimal oExpense = officeTxs.Where(t => t.Type == "Expense").Sum(t => Math.Abs(t.Amount));

                var approvedSubs = subs.Where(s => s.Status == SubscriptionStatus.Approved).ToList();
                int pendingSubsCount = subs.Count(s => s.Status == SubscriptionStatus.Pending);
                int rejectedSubsCount = subs.Count(s => s.Status == SubscriptionStatus.Rejected);

                // Site Financial Calculations:
                // 1. All subscriptions are site income
                decimal siteSubscriptionIncome = approvedSubs.Sum(s => s.Amount);

                // 2. Admin platform operating expenses
                var siteExpenseList = txs
                    .Where(t => t.UserId == adminId && t.Type == "Expense")
                    .OrderByDescending(t => t.Date)
                    .ToList();
                decimal siteExpensesTotal = siteExpenseList.Sum(t => Math.Abs(t.Amount));

                return new AdminOverviewStatistics
                {
                    TotalUsers = users.Count,
                    PersonalUsers = users.Count(u => u.AccountType.Equals("Personal", StringComparison.OrdinalIgnoreCase)),
                    OfficeUsers = users.Count(u => u.AccountType.Equals("Office", StringComparison.OrdinalIgnoreCase)),

                    TotalTransactions = txs.Count,
                    TotalTransactionVolume = txs.Sum(t => Math.Abs(t.Amount)),

                    PersonalTransactions = personalTxs.Count,
                    PersonalTotalIncome = pIncome,
                    PersonalTotalExpenses = pExpense,

                    OfficeTransactions = officeTxs.Count,
                    OfficeTotalIncome = oIncome,
                    OfficeTotalExpenses = oExpense,

                    ActiveOfficeSubscribers = approvedSubs.Count,
                    PendingSubscriptions = pendingSubsCount,
                    RejectedSubscriptions = rejectedSubsCount,
                    TotalSubscriptionRevenue = siteSubscriptionIncome,

                    // Site Financial Overview
                    SiteTotalIncome = siteSubscriptionIncome,
                    SiteTotalExpenses = siteExpensesTotal,
                    SiteExpenses = siteExpenseList
                };
            }
        }
    }
}
