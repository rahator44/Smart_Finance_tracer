using System;

namespace SmartFinanceManager.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty; // "Expense", "Income", "Transfer"
        public string Category { get; set; } = string.Empty; // "Food", "Salary", "Supplier Payment", etc.
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public bool IsOffice { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
