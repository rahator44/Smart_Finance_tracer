using System;
using System.Linq;
using Xunit;
using SmartFinanceManager.Services;
using SmartFinanceManager.Data;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Tests
{
    public class TransactionAndIsolationTests
    {
        private readonly TransactionService _transactionService;
        private readonly AuthService _authService;

        public TransactionAndIsolationTests()
        {
            _transactionService = new TransactionService();
            _authService = new AuthService();
            using var context = new FinanceDbContext();
            context.Database.EnsureCreated();
        }

        [Fact]
        public void TransactionCRUD_And_Filtering_WorksAccurately()
        {
            string email = $"crud_{Guid.NewGuid():N}@finance.com";
            _authService.Register("CRUD User", email, "password123", "Personal", out _);
            _authService.Login(email, "password123", false, out _);
            int userId = AuthService.CurrentUser!.Id;

            var targetMonth = new DateTime(2026, 9, 1);

            // 1. Add Transactions
            var tx1 = new Transaction
            {
                UserId = userId,
                Type = "Income",
                Category = "Salary & Wages",
                Amount = 5000,
                Date = new DateTime(2026, 9, 2),
                IsOffice = false,
                Description = "Monthly Tech Salary"
            };
            var tx2 = new Transaction
            {
                UserId = userId,
                Type = "Expense",
                Category = "Food & Dining",
                Amount = -150,
                Date = new DateTime(2026, 9, 5),
                IsOffice = false,
                Description = "Dinner with friends at Italian bistro"
            };
            var tx3 = new Transaction
            {
                UserId = userId,
                Type = "Expense",
                Category = "Housing",
                Amount = -1200,
                Date = new DateTime(2026, 9, 6),
                IsOffice = false,
                Description = "Apartment monthly lease"
            };
            var tx4 = new Transaction
            {
                UserId = userId,
                Type = "Transfer",
                Category = "Investments",
                Amount = -300,
                Date = new DateTime(2026, 9, 10),
                IsOffice = false,
                Description = "Savings transfer"
            };

            _transactionService.AddTransaction(tx1);
            _transactionService.AddTransaction(tx2);
            _transactionService.AddTransaction(tx3);
            _transactionService.AddTransaction(tx4);

            // 2. Query All Transactions for Month
            var allMonth = _transactionService.GetFilteredTransactions(userId, targetMonth, isOffice: false);
            Assert.Equal(4, allMonth.Count);

            // 3. Filter by Type
            var expensesOnly = _transactionService.GetFilteredTransactions(userId, targetMonth, isOffice: false, typeFilter: "Expense");
            Assert.Equal(2, expensesOnly.Count);
            Assert.All(expensesOnly, t => Assert.Equal("Expense", t.Type));

            var incomeOnly = _transactionService.GetFilteredTransactions(userId, targetMonth, isOffice: false, typeFilter: "Income");
            Assert.Single(incomeOnly);
            Assert.Equal("Salary & Wages", incomeOnly[0].Category);

            // 4. Filter by Search Query
            var searchResults = _transactionService.GetFilteredTransactions(userId, targetMonth, isOffice: false, searchTerm: "bistro");
            Assert.Single(searchResults);
            Assert.Equal("Food & Dining", searchResults[0].Category);

            // 5. Update Transaction
            var toUpdate = allMonth.First(t => t.Category == "Food & Dining");
            toUpdate.Amount = -180;
            toUpdate.Description = "Updated bistro dinner";
            bool updated = _transactionService.UpdateTransaction(toUpdate, userId);
            Assert.True(updated);

            var verifyUpdate = _transactionService.GetFilteredTransactions(userId, targetMonth, isOffice: false, searchTerm: "Updated");
            Assert.Single(verifyUpdate);
            Assert.Equal(-180, verifyUpdate[0].Amount);

            // 6. Delete Transaction
            bool deleted = _transactionService.DeleteTransaction(toUpdate.Id, userId);
            Assert.True(deleted);

            var afterDelete = _transactionService.GetFilteredTransactions(userId, targetMonth, isOffice: false);
            Assert.Equal(3, afterDelete.Count);
        }

        [Fact]
        public void UserIsolation_GuaranteesNoDataLeakage()
        {
            // Create User A
            string emailA = $"userA_{Guid.NewGuid():N}@finance.com";
            _authService.Register("User A", emailA, "password123", "Personal", out _);
            _authService.Login(emailA, "password123", false, out _);
            int userAId = AuthService.CurrentUser!.Id;

            // Create User B
            string emailB = $"userB_{Guid.NewGuid():N}@finance.com";
            _authService.Register("User B", emailB, "password123", "Personal", out _);
            _authService.Login(emailB, "password123", false, out _);
            int userBId = AuthService.CurrentUser!.Id;

            var testMonth = new DateTime(2026, 9, 1);

            // User A adds a confidential transaction
            var txA = new Transaction
            {
                UserId = userAId,
                Type = "Income",
                Category = "Salary & Wages",
                Amount = 10000,
                Date = testMonth,
                IsOffice = false,
                Description = "Confidential Salary for User A"
            };
            _transactionService.AddTransaction(txA);

            // User B queries transactions: MUST NOT see User A's transaction
            var userBTransactions = _transactionService.GetFilteredTransactions(userBId, testMonth, isOffice: false);
            Assert.Empty(userBTransactions);

            // User B attempts to tamper with User A's transaction: MUST FAIL
            txA.Amount = 99999;
            bool tamperUpdate = _transactionService.UpdateTransaction(txA, userId: userBId);
            Assert.False(tamperUpdate);

            // User B attempts to delete User A's transaction: MUST FAIL
            bool tamperDelete = _transactionService.DeleteTransaction(txA.Id, userId: userBId);
            Assert.False(tamperDelete);

            // User A's data remains safe and unmodified
            var userATransactions = _transactionService.GetFilteredTransactions(userAId, testMonth, isOffice: false);
            Assert.Single(userATransactions);
            Assert.Equal(10000, userATransactions[0].Amount);
        }
    }
}
