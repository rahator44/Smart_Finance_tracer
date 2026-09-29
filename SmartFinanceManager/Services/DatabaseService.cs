using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartFinanceManager.Data;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Services
{
    public class DatabaseService
    {
        public static void Initialize()
        {
            using (var context = new FinanceDbContext())
            {
                // Ensure db and schema are created
                context.Database.EnsureCreated();

                // Ensure Subscriptions table exists for backwards compatibility if database already existed
                try
                {
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS ""Subscriptions"" (
                            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Subscriptions"" PRIMARY KEY AUTOINCREMENT,
                            ""UserId"" INTEGER NOT NULL,
                            ""Email"" TEXT NOT NULL,
                            ""Amount"" TEXT NOT NULL,
                            ""TransactionId"" TEXT NOT NULL,
                            ""PaymentMethod"" TEXT NOT NULL,
                            ""Status"" INTEGER NOT NULL,
                            ""SubmittedDate"" TEXT NOT NULL,
                            ""ReviewedDate"" TEXT NULL,
                            ""ApprovedDate"" TEXT NULL,
                            ""RejectedDate"" TEXT NULL,
                            ""AdminNote"" TEXT NULL,
                            CONSTRAINT ""FK_Subscriptions_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_Subscriptions_User_Status"" ON ""Subscriptions"" (""UserId"", ""Status"");
                        CREATE INDEX IF NOT EXISTS ""IX_Subscriptions_TransactionId"" ON ""Subscriptions"" (""TransactionId"");
                    ");
                }
                catch { }

                // Seed default admin user or ensure admin password is set to 4444444444
                var adminUser = context.Users.FirstOrDefault(u => u.Email.ToLower() == AdminConfig.AdminEmail.ToLower());
                if (adminUser == null)
                {
                    context.Users.Add(new User
                    {
                        FullName = "Admin Rahat",
                        Email = AdminConfig.AdminEmail,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("4444444444"),
                        AccountType = "Office",
                        CreatedDate = DateTime.UtcNow
                    });
                    context.SaveChanges();
                }
                else
                {
                    adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("4444444444");
                    context.SaveChanges();
                }

                // Seed sample data if database has no other users
                if (context.Users.Count() <= 1)
                {
                    // 1. Create Personal User
                    var personalUser = context.Users.FirstOrDefault(u => u.Email.ToLower() == "personal@finance.com");
                    if (personalUser == null)
                    {
                        personalUser = new User
                        {
                            FullName = "Alex Morgan",
                            Email = "personal@finance.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                            AccountType = "Personal",
                            CreatedDate = DateTime.UtcNow
                        };
                        context.Users.Add(personalUser);
                    }

                    // 2. Create Office User
                    var officeUser = context.Users.FirstOrDefault(u => u.Email.ToLower() == "office@finance.com");
                    if (officeUser == null)
                    {
                        officeUser = new User
                        {
                            FullName = "Enterprise Corp Inc.",
                            Email = "office@finance.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                            AccountType = "Office",
                            CreatedDate = DateTime.UtcNow
                        };
                        context.Users.Add(officeUser);
                    }

                    context.SaveChanges();

                    int pId = personalUser.Id;
                    int oId = officeUser.Id;

                    // Seed approved subscription for demo office user
                    if (!context.Subscriptions.Any(s => s.UserId == oId))
                    {
                        context.Subscriptions.Add(new OfficeSubscription
                        {
                            UserId = oId,
                            Email = officeUser.Email,
                            Amount = 100m,
                            PaymentMethod = "bKash",
                            TransactionId = "BK9X77DEMO",
                            Status = SubscriptionStatus.Approved,
                            SubmittedDate = DateTime.UtcNow.AddDays(-30),
                            ReviewedDate = DateTime.UtcNow.AddDays(-30),
                            ApprovedDate = DateTime.UtcNow.AddDays(-30),
                            AdminNote = "Demo Account Initial Approval"
                        });
                    }

                    // Support both current month and reference mock month (2026-08 / 2026-09)
                    var mockDate = new DateTime(2026, 8, 15);
                    var today = DateTime.Today;

                    // Seed Personal transactions if none exist
                    if (!context.Transactions.Any(t => t.UserId == pId))
                    {
                        context.Transactions.AddRange(
                            new Transaction { UserId = pId, Type = "Income", Category = "Salary & Wages", Amount = 4500, Date = new DateTime(2026, 8, 1), IsOffice = false, Description = "Monthly salary paycheck" },
                            new Transaction { UserId = pId, Type = "Income", Category = "Bonus", Amount = 600, Date = new DateTime(2026, 8, 14), IsOffice = false, Description = "Performance reward bonus" },
                            new Transaction { UserId = pId, Type = "Expense", Category = "Food & Dining", Amount = -450, Date = new DateTime(2026, 8, 3), IsOffice = false, Description = "Whole Foods groceries and dining" },
                            new Transaction { UserId = pId, Type = "Expense", Category = "Housing", Amount = -1200, Date = new DateTime(2026, 8, 5), IsOffice = false, Description = "Apartment lease payment" },
                            new Transaction { UserId = pId, Type = "Expense", Category = "Transportation", Amount = -120, Date = new DateTime(2026, 8, 8), IsOffice = false, Description = "Gas refill & subway pass" },
                            new Transaction { UserId = pId, Type = "Expense", Category = "Entertainment", Amount = -85, Date = new DateTime(2026, 8, 12), IsOffice = false, Description = "Cinema and streaming subscriptions" },
                            new Transaction { UserId = pId, Type = "Expense", Category = "Phone & Internet", Amount = -95, Date = new DateTime(2026, 8, 16), IsOffice = false, Description = "Fiber internet and mobile plan" },
                            new Transaction { UserId = pId, Type = "Transfer", Category = "Investments", Amount = -500, Date = new DateTime(2026, 8, 20), IsOffice = false, Description = "Transfer to High-Yield Savings" }
                        );

                        if (today.Year != 2026 || today.Month != 8)
                        {
                            var firstOfMonth = new DateTime(today.Year, today.Month, 1);
                            context.Transactions.AddRange(
                                new Transaction { UserId = pId, Type = "Income", Category = "Salary & Wages", Amount = 4500, Date = firstOfMonth, IsOffice = false, Description = "Monthly salary direct deposit" },
                                new Transaction { UserId = pId, Type = "Expense", Category = "Food & Dining", Amount = -320, Date = today, IsOffice = false, Description = "Supermarket groceries" },
                                new Transaction { UserId = pId, Type = "Expense", Category = "Housing", Amount = -1200, Date = firstOfMonth.AddDays(2), IsOffice = false, Description = "Monthly rent payment" }
                            );
                        }
                    }

                    // Seed Office transactions if none exist
                    if (!context.Transactions.Any(t => t.UserId == oId))
                    {
                        context.Transactions.AddRange(
                            new Transaction { UserId = oId, Type = "Income", Category = "Client Sales", Amount = 22000, Date = new DateTime(2026, 8, 2), IsOffice = true, Description = "Enterprise SaaS Annual Licensing" },
                            new Transaction { UserId = oId, Type = "Income", Category = "Consulting Fees", Amount = 5500, Date = new DateTime(2026, 8, 11), IsOffice = true, Description = "Cloud migration consulting retainer" },
                            new Transaction { UserId = oId, Type = "Expense", Category = "Staff Salaries & Payroll", Amount = -8500, Date = new DateTime(2026, 8, 4), IsOffice = true, Description = "Engineering team monthly payroll" },
                            new Transaction { UserId = oId, Type = "Expense", Category = "Office Rent & Lease", Amount = -3200, Date = new DateTime(2026, 8, 5), IsOffice = true, Description = "Downtown office space commercial lease" },
                            new Transaction { UserId = oId, Type = "Expense", Category = "Software & SaaS", Amount = -1400, Date = new DateTime(2026, 8, 7), IsOffice = true, Description = "AWS Cloud infrastructure and tools" },
                            new Transaction { UserId = oId, Type = "Expense", Category = "Marketing & Ads", Amount = -1800, Date = new DateTime(2026, 8, 9), IsOffice = true, Description = "Google & LinkedIn targeted ad campaigns" },
                            new Transaction { UserId = oId, Type = "Expense", Category = "Utilities & Fiber", Amount = -420, Date = new DateTime(2026, 8, 12), IsOffice = true, Description = "Gigabit fiber internet & office power" },
                            new Transaction { UserId = oId, Type = "Transfer", Category = "Treasury", Amount = 3000, Date = new DateTime(2026, 8, 18), IsOffice = true, Description = "Operating reserve capital injection" }
                        );

                        if (today.Year != 2026 || today.Month != 8)
                        {
                            var firstOfMonth = new DateTime(today.Year, today.Month, 1);
                            context.Transactions.AddRange(
                                new Transaction { UserId = oId, Type = "Income", Category = "Client Sales", Amount = 19500, Date = firstOfMonth, IsOffice = true, Description = "Quarterly client retainer" },
                                new Transaction { UserId = oId, Type = "Expense", Category = "Staff Salaries & Payroll", Amount = -8500, Date = firstOfMonth.AddDays(3), IsOffice = true, Description = "Monthly staff payroll" },
                                new Transaction { UserId = oId, Type = "Expense", Category = "Office Rent & Lease", Amount = -3200, Date = firstOfMonth.AddDays(5), IsOffice = true, Description = "Commercial office lease" }
                            );
                        }
                    }

                    // Seed Budgets
                    if (!context.Budgets.Any(b => b.UserId == pId && b.Month == "2026-08"))
                    {
                        context.Budgets.Add(new Budget { UserId = pId, Month = "2026-08", TargetAmount = 2800, IsOffice = false });
                    }

                    if (!context.Budgets.Any(b => b.UserId == oId && b.Month == "2026-08"))
                    {
                        context.Budgets.Add(new Budget { UserId = oId, Month = "2026-08", TargetAmount = 18000, IsOffice = true });
                    }

                    context.SaveChanges();
                }
            }
        }
    }
}
