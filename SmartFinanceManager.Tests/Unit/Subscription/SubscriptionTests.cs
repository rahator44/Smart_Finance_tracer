// [Rahat Test Suite] Corporate KPI metrics, cash burn rate & subscription lifecycle verification
using System;
using System.Linq;
using Xunit;
using SmartFinanceManager.Data;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.Tests.Unit.Subscription
{
    public class SubscriptionTests
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly AuthService _authService;
        private readonly TransactionService _transactionService;

        public SubscriptionTests()
        {
            _subscriptionService = new SubscriptionService();
            _authService = new AuthService();
            _transactionService = new TransactionService();

            using var context = new FinanceDbContext();
            DatabaseService.Initialize();
        }

        private User CreateTestUser(string name, string role = "Personal", string? customEmail = null)
        {
            string email = customEmail ?? $"sub_user_{Guid.NewGuid():N}@finance.com";
            _authService.Register(name, email, "password123", role, out _);
            using var context = new FinanceDbContext();
            return context.Users.First(u => u.Email.ToLower() == email.ToLower());
        }

        private User CreateAdminUser()
        {
            using var context = new FinanceDbContext();
            var existing = context.Users.FirstOrDefault(u => u.Email.ToLower() == AdminConfig.AdminEmail.ToLower());
            if (existing != null) return existing;

            _authService.Register("Super Admin", AdminConfig.AdminEmail, "AdminPassword123", "Office", out _);
            return context.Users.First(u => u.Email.ToLower() == AdminConfig.AdminEmail.ToLower());
        }

        [Fact]
        public void Scenario01_02_05_CreateSubscriptionRequest_AssociatesUserId_AndSetsPending()
        {
            // Scenario 1: Creating a subscription request
            // Scenario 2: Correct UserId association
            // Scenario 5: Pending status immediately after submission
            var user = CreateTestUser("John Doe");
            string txId = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (success, message, sub) = _subscriptionService.CreateSubscriptionRequest(
                user.Id, user.Email, 100m, txId, "bKash");

            Assert.True(success);
            Assert.NotNull(sub);
            Assert.Equal(user.Id, sub!.UserId);
            Assert.Equal(user.Email.ToLower(), sub.Email.ToLower());
            Assert.Equal(100m, sub.Amount);
            Assert.Equal(txId, sub.TransactionId);
            Assert.Equal(SubscriptionStatus.Pending, sub.Status);
            Assert.Equal(SubscriptionStatus.Pending, _subscriptionService.GetSubscriptionStatus(user.Id));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        [InlineData(200)]
        public void Scenario03_AmountValidation_RequiresExactSubscriptionFee(decimal invalidAmount)
        {
            // Scenario 3: Amount validation (Must be 100 BDT)
            var user = CreateTestUser("Amount Test User");
            string txId = $"TX_{Guid.NewGuid():N}".Substring(0, 8);

            var (success, message, sub) = _subscriptionService.CreateSubscriptionRequest(
                user.Id, user.Email, invalidAmount, txId, "bKash");

            Assert.False(success);
            Assert.Null(sub);
            Assert.Contains("100", message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("AB")] // Too short
        [InlineData(null)]
        public void Scenario04_TransactionIdValidation_RejectsInvalidInput(string? invalidTxId)
        {
            // Scenario 4: Transaction ID validation
            var user = CreateTestUser("TxId Test User");

            var (success, message, sub) = _subscriptionService.CreateSubscriptionRequest(
                user.Id, user.Email, 100m, invalidTxId!, "bKash");

            Assert.False(success);
            Assert.Null(sub);
        }

        [Fact]
        public void Scenario06_ApprovedSubscription_GrantsOfficeAccess()
        {
            // Scenario 6: Approved subscription grants Office access
            var user = CreateTestUser("Approved User");
            var admin = CreateAdminUser();
            string txId = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (createSuccess, _, sub) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId);
            Assert.True(createSuccess);

            // Approve via Admin
            bool approved = _subscriptionService.ApproveSubscription(sub!.Id, admin.Id, "Payment Verified via bKash", out _);
            Assert.True(approved);

            // Verify access
            Assert.True(_subscriptionService.HasActiveOfficeSubscription(user.Id));
            Assert.True(_subscriptionService.HasOfficeAccess(user));
            Assert.Equal(SubscriptionStatus.Approved, _subscriptionService.GetSubscriptionStatus(user.Id));
        }

        [Fact]
        public void Scenario07_PendingSubscription_DoesNotGrantOfficeAccess()
        {
            // Scenario 7: Pending subscription does not grant Office access
            var user = CreateTestUser("Pending User");
            string txId = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId);

            Assert.False(_subscriptionService.HasActiveOfficeSubscription(user.Id));
            Assert.False(_subscriptionService.HasOfficeAccess(user));
        }

        [Fact]
        public void Scenario08_RejectedSubscription_DoesNotGrantOfficeAccess()
        {
            // Scenario 8: Rejected subscription does not grant Office access
            var user = CreateTestUser("Rejected User");
            var admin = CreateAdminUser();
            string txId = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (_, _, sub) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId);
            _subscriptionService.RejectSubscription(sub!.Id, admin.Id, "Invalid transaction ID or amount", out _);

            Assert.False(_subscriptionService.HasActiveOfficeSubscription(user.Id));
            Assert.False(_subscriptionService.HasOfficeAccess(user));
            Assert.Equal(SubscriptionStatus.Rejected, _subscriptionService.GetSubscriptionStatus(user.Id));
        }

        [Fact]
        public void Scenario09_RejectedSubscription_CanBeResubmitted()
        {
            // Scenario 9: Rejected subscription can be resubmitted
            var user = CreateTestUser("Resubmit User");
            var admin = CreateAdminUser();
            string txId1 = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();
            string txId2 = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            // First submission rejected
            var (_, _, sub1) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId1);
            _subscriptionService.RejectSubscription(sub1!.Id, admin.Id, "Typo in TxID", out _);

            // Resubmit with new TxID
            var (resubmitSuccess, message, sub2) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId2);
            Assert.True(resubmitSuccess);
            Assert.NotNull(sub2);
            Assert.Equal(SubscriptionStatus.Pending, sub2!.Status);
            Assert.Equal(SubscriptionStatus.Pending, _subscriptionService.GetSubscriptionStatus(user.Id));
        }

        [Fact]
        public void Scenario10_DuplicatePendingSubscriptions_ArePrevented()
        {
            // Scenario 10: Duplicate pending subscriptions are prevented
            var user = CreateTestUser("Duplicate Test User");
            string txId1 = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();
            string txId2 = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (firstSuccess, _, _) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId1);
            Assert.True(firstSuccess);

            // Second submission while first is still pending
            var (secondSuccess, secondMsg, _) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId2);
            Assert.False(secondSuccess);
            Assert.Contains("pending", secondMsg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Scenario11_NormalUser_CannotApproveSubscriptions()
        {
            // Scenario 11: Normal user cannot approve subscriptions
            var applicant = CreateTestUser("Applicant");
            var attacker = CreateTestUser("Normal Attacker");
            string txId = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (_, _, sub) = _subscriptionService.CreateSubscriptionRequest(applicant.Id, applicant.Email, 100m, txId);

            // Attacker tries to approve applicant's subscription
            bool result = _subscriptionService.ApproveSubscription(sub!.Id, attacker.Id, "Self Approve Attempt", out string msg);
            Assert.False(result);
            Assert.Contains("Unauthorized", msg);
            Assert.False(_subscriptionService.HasActiveOfficeSubscription(applicant.Id));
        }

        [Fact]
        public void Scenario12_UserIsolation_GuaranteesUserOnlySeesOwnSubscription()
        {
            // Scenario 12: Normal user cannot access another user's subscription
            var userA = CreateTestUser("User Alpha");
            var userB = CreateTestUser("User Beta");
            string txIdA = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            _subscriptionService.CreateSubscriptionRequest(userA.Id, userA.Email, 100m, txIdA);

            var userBSub = _subscriptionService.GetLatestUserSubscription(userB.Id);
            Assert.Null(userBSub);

            var userASub = _subscriptionService.GetLatestUserSubscription(userA.Id);
            Assert.NotNull(userASub);
            Assert.Equal(txIdA, userASub!.TransactionId);
        }

        [Fact]
        public void Scenario13_Admin_CanApproveSubscriptions()
        {
            // Scenario 13: Admin can approve subscriptions
            var user = CreateTestUser("Admin Approval Target");
            var admin = CreateAdminUser();
            string txId = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (_, _, sub) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId);

            bool approved = _subscriptionService.ApproveSubscription(sub!.Id, admin.Id, "Verified on bKash merchant statement", out string msg);
            Assert.True(approved);
            Assert.Contains("approved", msg, StringComparison.OrdinalIgnoreCase);

            var updatedSub = _subscriptionService.GetLatestUserSubscription(user.Id);
            Assert.Equal(SubscriptionStatus.Approved, updatedSub!.Status);
            Assert.NotNull(updatedSub.ApprovedDate);
            Assert.Equal("Verified on bKash merchant statement", updatedSub.AdminNote);
        }

        [Fact]
        public void Scenario14_Admin_CanRejectSubscriptions_WithOptionalNote()
        {
            // Scenario 14: Admin can reject subscriptions
            var user = CreateTestUser("Admin Rejection Target");
            var admin = CreateAdminUser();
            string txId = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (_, _, sub) = _subscriptionService.CreateSubscriptionRequest(user.Id, user.Email, 100m, txId);

            bool rejected = _subscriptionService.RejectSubscription(sub!.Id, admin.Id, "Transaction ID not found in bKash statement", out string msg);
            Assert.True(rejected);

            var updatedSub = _subscriptionService.GetLatestUserSubscription(user.Id);
            Assert.Equal(SubscriptionStatus.Rejected, updatedSub!.Status);
            Assert.NotNull(updatedSub.RejectedDate);
            Assert.Equal("Transaction ID not found in bKash statement", updatedSub.AdminNote);
        }

        [Fact]
        public void Scenario15_Admin_CanAccessOfficeFinance_WithoutSubscription()
        {
            // Scenario 15: Admin can access Office Finance without subscription
            var admin = CreateAdminUser();

            // Admin has no subscription in database
            Assert.False(_subscriptionService.HasActiveOfficeSubscription(admin.Id));

            // But HasOfficeAccess returns true because admin bypasses subscription
            Assert.True(AdminConfig.IsAdmin(admin));
            Assert.True(_subscriptionService.HasOfficeAccess(admin));
        }

        [Fact]
        public void Scenario16_PersonalFinance_RemainsAccessible_WithoutSubscription()
        {
            // Scenario 16: Personal Finance remains accessible without subscription
            var user = CreateTestUser("Free Tier User");

            // User has no subscription
            Assert.False(_subscriptionService.HasActiveOfficeSubscription(user.Id));
            Assert.False(_subscriptionService.HasOfficeAccess(user));

            // User can still freely perform Personal Finance transactions
            var tx = new Transaction
            {
                UserId = user.Id,
                Type = "Income",
                Category = "Salary & Wages",
                Amount = 3500,
                Date = DateTime.Today,
                IsOffice = false,
                Description = "Free Tier Personal Salary"
            };

            _transactionService.AddTransaction(tx);

            var summary = _transactionService.GetMonthlySummary(user.Id, DateTime.Today, isOffice: false);
            Assert.Equal(3500, summary.TotalIncome);
            Assert.Equal(3500, summary.Balance);

            var userTxs = _transactionService.GetFilteredTransactions(user.Id, DateTime.Today, isOffice: false);
            Assert.Single(userTxs);
        }

        [Fact]
        public void Scenario17_AdminUser_CanLoginWith_ConfiguredPassword_4444444444()
        {
            DatabaseService.Initialize();
            bool loginSuccess = _authService.Login(AdminConfig.AdminEmail, "4444444444", false, out string msg);
            Assert.True(loginSuccess, $"Admin login failed: {msg}");
            Assert.NotNull(AuthService.CurrentUser);
            Assert.True(AdminConfig.IsAdmin(AuthService.CurrentUser));
        }

        [Fact]
        public void Scenario18_SiteIncome_CalculatedFromAllApprovedSubscriptions()
        {
            var admin = CreateAdminUser();
            var user1 = CreateTestUser("Subscriber 1");
            var user2 = CreateTestUser("Subscriber 2");

            string txId1 = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();
            string txId2 = $"BK_{Guid.NewGuid():N}".Substring(0, 10).ToUpper();

            var (_, _, sub1) = _subscriptionService.CreateSubscriptionRequest(user1.Id, user1.Email, 100m, txId1);
            var (_, _, sub2) = _subscriptionService.CreateSubscriptionRequest(user2.Id, user2.Email, 100m, txId2);

            _subscriptionService.ApproveSubscription(sub1!.Id, admin.Id, "Approved 1", out _);
            _subscriptionService.ApproveSubscription(sub2!.Id, admin.Id, "Approved 2", out _);

            var stats = _subscriptionService.GetAdminOverviewStatistics();
            Assert.True(stats.SiteTotalIncome >= 200m);
            Assert.Equal(stats.TotalSubscriptionRevenue, stats.SiteTotalIncome);
        }

        [Fact]
        public void Scenario19_Admin_CanAddAndPersist_SiteOperatingExpense_InRuntime()
        {
            var admin = CreateAdminUser();

            var (success, msg, tx) = _subscriptionService.AddSiteOperatingExpense(
                admin.Id,
                550m,
                "Cloud Server & Hosting",
                "DigitalOcean Droplet Production VPS",
                DateTime.Today);

            Assert.True(success);
            Assert.NotNull(tx);
            Assert.Equal(-550m, tx!.Amount);
            Assert.Equal("Cloud Server & Hosting", tx.Category);

            var expenses = _subscriptionService.GetSiteOperatingExpenses(admin.Id);
            Assert.Contains(expenses, e => e.Id == tx.Id && e.Category == "Cloud Server & Hosting");

            var stats = _subscriptionService.GetAdminOverviewStatistics();
            Assert.True(stats.SiteTotalExpenses >= 550m);
            Assert.Equal(stats.SiteTotalIncome - stats.SiteTotalExpenses, stats.SiteNetProfit);
        }

        [Fact]
        public void Scenario20_Admin_CanDeleteSiteOperatingExpense()
        {
            var admin = CreateAdminUser();

            var (success, _, tx) = _subscriptionService.AddSiteOperatingExpense(
                admin.Id,
                120m,
                "Domain & DNS",
                "Cloudflare domain renew",
                DateTime.Today);

            Assert.True(success);
            Assert.NotNull(tx);

            bool deleted = _subscriptionService.DeleteSiteOperatingExpense(tx!.Id, admin.Id);
            Assert.True(deleted);

            var expenses = _subscriptionService.GetSiteOperatingExpenses(admin.Id);
            Assert.DoesNotContain(expenses, e => e.Id == tx.Id);
        }

        [Fact]
        public void Scenario21_NormalUser_CannotAddSiteOperatingExpense()
        {
            var normalUser = CreateTestUser("Hacker Attempt");

            var (success, msg, tx) = _subscriptionService.AddSiteOperatingExpense(
                normalUser.Id,
                300m,
                "Cloud Server",
                "Unauthorized expense attempt",
                DateTime.Today);

            Assert.False(success);
            Assert.Null(tx);
            Assert.Contains("Unauthorized", msg);
        }
    }
}
