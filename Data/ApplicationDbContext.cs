using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<UserExpense> UserExpenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntity(modelBuilder);
        ConfigureExpenseEntity(modelBuilder);
        ConfigureUserExpenseEntity(modelBuilder);
    }
}
