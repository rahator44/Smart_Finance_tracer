namespace SmartFinanceManager.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Month { get; set; } = string.Empty; // "YYYY-MM"
        public decimal TargetAmount { get; set; }
        public bool IsOffice { get; set; }
    }
}
