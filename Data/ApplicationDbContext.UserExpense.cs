using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Data;

public partial class ApplicationDbContext
{
    private void ConfigureUserExpenseEntity(ModelBuilder modelBuilder)
    {
        // Configure UserExpense table (junction table)
        modelBuilder.Entity<UserExpense>(entity =>
        {
            entity.ToTable("user_expenses", "public");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(26).IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(26).IsRequired();
            entity
                .Property(e => e.ExpenseId)
                .HasColumnName("expense_id")
                .HasMaxLength(26)
                .IsRequired();
            entity
                .Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key constraints
            entity
                .HasOne(ue => ue.User)
                .WithMany(u => u.UserExpenses)
                .HasForeignKey(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(ue => ue.Expense)
                .WithMany(e => e.UserExpenses)
                .HasForeignKey(ue => ue.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index untuk query by user_id dan expense_id
            entity.HasIndex(ue => ue.UserId);
            entity.HasIndex(ue => ue.ExpenseId);
        });
    }
}
