namespace AccountingApp.Models
{
    public class UserExpense
    {
        public string Id { get; set; } = string.Empty; // ULID
        public string UserId { get; set; } = string.Empty;
        public string ExpenseId { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Expense? Expense { get; set; }
    }
}
