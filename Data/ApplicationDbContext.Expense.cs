using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Data;

public partial class ApplicationDbContext
{
    private void ConfigureExpenseEntity(ModelBuilder modelBuilder)
    {
        // Configure Expense table
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("expenses", "public");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(26).IsRequired();
            entity
                .Property(e => e.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric")
                .IsRequired();
            entity
                .Property(e => e.ExpenseDate)
                .HasColumnName("expense_date")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity
                .Property(e => e.PaymentMethod)
                .HasColumnName("payment_method")
                .IsRequired()
                .HasMaxLength(16);
            entity
                .Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index untuk query by date
            entity.HasIndex(e => e.ExpenseDate);

            // Configure relationships
            entity
                .HasMany(e => e.UserExpenses)
                .WithOne(ue => ue.Expense)
                .HasForeignKey(ue => ue.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
