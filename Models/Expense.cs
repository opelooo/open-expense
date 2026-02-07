namespace AccountingApp.Models
{
    public class Expense
    {
        public string Id { get; set; } = string.Empty; // ULID
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation property
        public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();
    }
}
