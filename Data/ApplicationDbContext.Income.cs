using Microsoft.EntityFrameworkCore;
using OpenExpenseApp.Models;

namespace OpenExpenseApp.Data;

public partial class ApplicationDbContext
{
    private void ConfigureIncomeEntity(ModelBuilder modelBuilder)
    {
        // Configure Income table
        modelBuilder.Entity<Income>(entity =>
        {
            entity.ToTable("incomes", "public");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(26).IsRequired();

            entity.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(26).IsRequired();

            entity
                .Property(e => e.SourceName)
                .HasColumnName("source_name")
                .HasMaxLength(50)
                .IsRequired();

            entity
                .Property(e => e.IncomeType)
                .HasColumnName("income_type")
                .HasMaxLength(50)
                .IsRequired();

            entity
                .Property(e => e.GrossAmount)
                .HasColumnName("gross_amount")
                .HasColumnType("numeric")
                .IsRequired();

            entity
                .Property(e => e.TaxWithheld)
                .HasColumnName("tax_withheld")
                .HasColumnType("numeric");

            entity
                .Property(e => e.ReceivedDate)
                .HasColumnName("received_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(e => e.IsTaxable).HasColumnName("is_taxable").HasDefaultValue(true);

            entity
                .Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
