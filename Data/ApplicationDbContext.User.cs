using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Data;

public partial class ApplicationDbContext
{
    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        // Configure User table
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users_account", "public");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasMaxLength(26).IsRequired();
            entity
                .Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(16);
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
            entity
                .Property(e => e.Password)
                .HasColumnName("password")
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity
                .Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();
            entity
                .Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Add index untuk query by username (biasanya dipakai di login)
            entity.HasIndex(e => e.Username).IsUnique();

            // Configure relationships
            entity
                .HasMany(u => u.UserExpenses)
                .WithOne(ue => ue.User)
                .HasForeignKey(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
