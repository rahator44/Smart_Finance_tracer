// [Rupom Security Architecture] Complete Authentication & Session Caching Engine
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartFinanceManager.Data;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Services
{
    public class UserStatistics
    {
        public int TotalTransactions { get; set; }
        public decimal LifetimeIncome { get; set; }
        public decimal LifetimeExpenses { get; set; }
        public DateTime MemberSince { get; set; }
    }

    public class AuthService
    {
        private static readonly string SessionFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "SmartFinanceManager", "session.dat");

        // Holds the currently logged in user session
        public static User? CurrentUser { get; set; }

        public static bool ValidateEmail(string email, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                error = "Email address is required.";
                return false;
            }

            string cleanEmail = email.Trim().ToLowerInvariant();
            int atIndex = cleanEmail.IndexOf('@');
            if (atIndex <= 0 || atIndex != cleanEmail.LastIndexOf('@'))
            {
                error = "Invalid email format. Must contain a single '@'.";
                return false;
            }

            string domain = cleanEmail.Substring(atIndex + 1);
            if (!domain.Contains("."))
            {
                error = "Invalid domain format. Email must contain a domain (e.g. .com).";
                return false;
            }

            // Strict Gmail check: If email domain contains 'gmail', it MUST end with 'gmail.com'
            if (domain.Contains("gmail") && !domain.EndsWith("gmail.com"))
            {
                error = "Invalid Gmail address. Gmail accounts must end with 'gmail.com'.";
                return false;
            }

            int lastDot = domain.LastIndexOf('.');
            string extension = domain.Substring(lastDot + 1);
            if (extension.Length < 2)
            {
                error = "Invalid domain extension (e.g. .com, .org, .net).";
                return false;
            }

            return true;
        }

        public static bool ValidatePassword(string password, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Password is required.";
                return false;
            }

            if (password.Length < 6)
            {
                error = "Password must be at least 6 characters long.";
                return false;
            }

            return true;
        }

        public static void SaveSession(string email)
        {
            try
            {
                string dir = Path.GetDirectoryName(SessionFilePath)!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(SessionFilePath, email.Trim().ToLowerInvariant());
            }
            catch { }
        }

        public static void ClearSession()
        {
            try
            {
                if (File.Exists(SessionFilePath))
                {
                    File.Delete(SessionFilePath);
                }
            }
            catch { }
        }

        public static bool TryRestoreSession()
        {
            try
            {
                if (File.Exists(SessionFilePath))
                {
                    string email = File.ReadAllText(SessionFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        using (var context = new FinanceDbContext())
                        {
                            var user = context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
                            if (user != null)
                            {
                                CurrentUser = user;
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public bool Register(string fullName, string email, string password, string accountType, out string message)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                message = "All fields are required.";
                return false;
            }

            if (fullName.Trim().Length < 2)
            {
                message = "Full name must be at least 2 characters.";
                return false;
            }

            if (!ValidateEmail(email, out string emailError))
            {
                message = emailError;
                return false;
            }

            if (!ValidatePassword(password, out string passError))
            {
                message = passError;
                return false;
            }

            using (var context = new FinanceDbContext())
            {
                var existingUser = context.Users.FirstOrDefault(u => u.Email.ToLower() == email.Trim().ToLower());
                if (existingUser != null)
                {
                    message = "A user with this email address already exists.";
                    return false;
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                var newUser = new User
                {
                    FullName = fullName.Trim(),
                    Email = email.Trim().ToLower(),
                    PasswordHash = passwordHash,
                    AccountType = accountType,
                    CreatedDate = DateTime.UtcNow
                };

                context.Users.Add(newUser);
                context.SaveChanges();
            }

            message = "Registration successful.";
            return true;
        }

        public bool Login(string email, string password, string expectedAccountType, bool rememberMe, out string message)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                message = "Email and Password are required.";
                return false;
            }

            if (!ValidateEmail(email, out string emailError))
            {
                message = emailError;
                return false;
            }

            using (var context = new FinanceDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Email.ToLower() == email.Trim().ToLower());
                if (user == null)
                {
                    message = "Invalid email or password.";
                    return false;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    message = "Invalid email or password.";
                    return false;
                }

                // Strict Account Type Match Verification
                if (!string.IsNullOrWhiteSpace(expectedAccountType) && 
                    !user.AccountType.Equals(expectedAccountType, StringComparison.OrdinalIgnoreCase))
                {
                    message = $"Account Type Mismatch: This account is registered as '{user.AccountType}'. Please select '{user.AccountType}' mode to log in.";
                    return false;
                }

                CurrentUser = user;

                if (rememberMe)
                {
                    SaveSession(user.Email);
                }
                else
                {
                    ClearSession();
                }
            }

            message = "Login successful.";
            return true;
        }

        public bool Login(string email, string password, bool rememberMe, out string message)
        {
            return Login(email, password, string.Empty, rememberMe, out message);
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                message = "Current and new passwords are required.";
                return false;
            }

            if (!ValidatePassword(newPassword, out string passErr))
            {
                message = passErr;
                return false;
            }

            using (var context = new FinanceDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    message = "User not found.";
                    return false;
                }

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    message = "Current password is incorrect.";
                    return false;
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                context.SaveChanges();
            }

            message = "Password updated successfully.";
            return true;
        }

        public bool UpdateProfile(int userId, string newFullName, out string message)
        {
            if (string.IsNullOrWhiteSpace(newFullName) || newFullName.Trim().Length < 2)
            {
                message = "Full name must be at least 2 characters.";
                return false;
            }

            using (var context = new FinanceDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    message = "User not found.";
                    return false;
                }

                user.FullName = newFullName.Trim();
                context.SaveChanges();

                if (CurrentUser != null && CurrentUser.Id == userId)
                {
                    CurrentUser.FullName = user.FullName;
                }
            }

            message = "Profile updated successfully.";
            return true;
        }

        public UserStatistics GetUserStatistics(int userId)
        {
            using (var context = new FinanceDbContext())
            {
                var user = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
                var txs = context.Transactions.AsNoTracking().Where(t => t.UserId == userId).ToList();

                decimal totalIncome = txs.Where(t => t.Type == "Income").Sum(t => Math.Abs(t.Amount));
                decimal totalExpenses = txs.Where(t => t.Type == "Expense").Sum(t => Math.Abs(t.Amount));

                return new UserStatistics
                {
                    TotalTransactions = txs.Count,
                    LifetimeIncome = totalIncome,
                    LifetimeExpenses = totalExpenses,
                    MemberSince = user?.CreatedDate ?? DateTime.UtcNow
                };
            }
        }

        public void Logout()
        {
            ClearSession();
            CurrentUser = null;
        }
    }
}
