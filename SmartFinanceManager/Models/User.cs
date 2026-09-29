using System;

namespace SmartFinanceManager.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // "Personal" or "Office"
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
