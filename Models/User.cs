namespace AccountingApp.Models;

public class User
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public ICollection<UserExpense> UserExpenses { get; set; } = new List<UserExpense>();
}
