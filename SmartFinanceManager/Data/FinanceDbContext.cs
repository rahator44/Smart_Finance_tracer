using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Data
{
    public class FinanceDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<OfficeSubscription> Subscriptions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Always locate FinanceManager.db relative to application base directory for Visual Studio & Windows compatibility
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FinanceManager.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique index for Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Transaction foreign key and indexes
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => new { t.UserId, t.IsOffice, t.Date })
                      .HasDatabaseName("IX_Transactions_User_Office_Date");
            });

            // Budget foreign key and unique monthly budget per user/mode
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(b => new { b.UserId, b.Month, b.IsOffice })
                      .IsUnique()
                      .HasDatabaseName("IX_Budgets_User_Month_Office");
            });

            // OfficeSubscription foreign key and indexes
            modelBuilder.Entity<OfficeSubscription>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => new { s.UserId, s.Status })
                      .HasDatabaseName("IX_Subscriptions_User_Status");

                entity.HasIndex(s => s.TransactionId)
                      .HasDatabaseName("IX_Subscriptions_TransactionId");
            });
        }
    }
}
