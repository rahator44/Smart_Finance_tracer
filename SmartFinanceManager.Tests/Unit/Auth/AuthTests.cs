using System;
using Xunit;
using SmartFinanceManager.Services;
using SmartFinanceManager.Data;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Tests
{
    public class AuthTests
    {
        private readonly AuthService _authService;

        public AuthTests()
        {
            _authService = new AuthService();
            // Ensure DB is created
            using var context = new FinanceDbContext();
            context.Database.EnsureCreated();
        }

        [Fact]
        public void ValidateEmail_ValidAddresses_ReturnsTrue()
        {
            Assert.True(AuthService.ValidateEmail("user@example.com", out _));
            Assert.True(AuthService.ValidateEmail("john.doe@finance.org", out _));
            Assert.True(AuthService.ValidateEmail("test@gmail.com", out _));
        }

        [Fact]
        public void ValidateEmail_InvalidAddresses_ReturnsFalse()
        {
            Assert.False(AuthService.ValidateEmail("noatsign.com", out string err1));
            Assert.Contains("Must contain a single '@'", err1);

            Assert.False(AuthService.ValidateEmail("two@@atsigns.com", out string err2));
            Assert.Contains("Must contain a single '@'", err2);

            Assert.False(AuthService.ValidateEmail("test@gmail.co", out string err3));
            Assert.Contains("Gmail accounts must end with 'gmail.com'", err3);
        }

        [Fact]
        public void ValidatePassword_LessThanSixCharacters_ReturnsFalse()
        {
            Assert.False(AuthService.ValidatePassword("12345", out string err));
            Assert.Contains("at least 6 characters", err);

            Assert.True(AuthService.ValidatePassword("123456", out _));
        }

        [Fact]
        public void RegisterAndLogin_ValidCredentials_Succeeds()
        {
            string uniqueEmail = $"test_{Guid.NewGuid():N}@finance.com";
            string password = "StrongPassword123";

            bool registered = _authService.Register("Test Tester", uniqueEmail, password, "Personal", out string regMsg);
            Assert.True(registered, regMsg);

            // Cannot register again with same email
            bool duplicate = _authService.Register("Test Tester", uniqueEmail, password, "Personal", out string dupMsg);
            Assert.False(duplicate);
            Assert.Contains("already exists", dupMsg);

            // Successful login with correct mode
            bool loggedIn = _authService.Login(uniqueEmail, password, "Personal", rememberMe: false, out string loginMsg);
            Assert.True(loggedIn, loginMsg);
            Assert.NotNull(AuthService.CurrentUser);
            Assert.Equal("Test Tester", AuthService.CurrentUser!.FullName);

            // Mismatched mode rejected
            bool modeMismatch = _authService.Login(uniqueEmail, password, "Office", rememberMe: false, out string mismatchMsg);
            Assert.False(modeMismatch);
            Assert.Contains("Account Type Mismatch", mismatchMsg);
        }

        [Fact]
        public void ChangePassword_ValidOldPassword_UpdatesSuccessfully()
        {
            string email = $"pwd_{Guid.NewGuid():N}@finance.com";
            _authService.Register("Password User", email, "oldPassword123", "Personal", out _);
            _authService.Login(email, "oldPassword123", false, out _);

            int userId = AuthService.CurrentUser!.Id;

            // Incorrect current password
            bool failed = _authService.ChangePassword(userId, "wrongPassword", "newPassword123", out string failMsg);
            Assert.False(failed);
            Assert.Contains("incorrect", failMsg);

            // Correct current password
            bool success = _authService.ChangePassword(userId, "oldPassword123", "newPassword123", out string succMsg);
            Assert.True(success, succMsg);

            // Can now log in with new password
            bool newLogin = _authService.Login(email, "newPassword123", false, out _);
            Assert.True(newLogin);
        }
    }
}
