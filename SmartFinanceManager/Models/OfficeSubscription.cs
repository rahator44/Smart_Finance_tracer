using System;

namespace SmartFinanceManager.Models
{
    public enum SubscriptionStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class OfficeSubscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 100m;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "bKash";
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? AdminNote { get; set; }

        // Navigation property for EF Core
        public User? User { get; set; }
    }
}
