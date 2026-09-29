using System;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Helpers
{
    public static class AdminConfig
    {
        // Centralized administrator Gmail address
        public const string AdminEmail = "rahator44@gmail.com";

        // Payment Configuration
        public const decimal OfficeSubscriptionPrice = 100m;
        public const string BkashRecipientNumber = "01903247467";

        public static bool IsAdmin(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return string.Equals(email.Trim(), AdminEmail, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAdmin(User? user)
        {
            if (user == null) return false;
            return IsAdmin(user.Email);
        }
    }
}
