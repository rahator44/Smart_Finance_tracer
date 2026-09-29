# Smart Finance Tracer 🚀💼

A modern, enterprise-grade WPF desktop financial management platform built with .NET 8, SQLite, and Entity Framework Core. Engineered for both personal pocket tracking and corporate office financial operations.

## 👥 Contributors & Core Modules

| Contributor | GitHub Account | Branch | Responsibilities & Modules |
| :--- | :--- | :--- | :--- |
| **Rahat (Lead)** | [@rahator44](https://github.com/rahator44) | `Rahat` | **Office Finance (Frontend & Backend)**: Corporate dashboards, enterprise runway, burn rate, payroll, B2B sales, multi-tenant switching. |
| **Farabi** | [@mahdiazfar27](https://github.com/mahdiazfar27) | `Farabi` | **Pocket Tracer Backend & Administration**: SQLite EF Core engine, TransactionService, Admin Portal treasury oversight, license audit. |
| **Rupom** | [@rupamimamhasan-86](https://github.com/rupamimamhasan-86) | `Rupom` | **Authentication & Notifications**: User security, login & registration UI/MVVM, password hashing, and floating toast notification banner. |
| **Raven (Redwan)** | [@raven-0-2](https://github.com/raven-0-2) | `Redwan` | **Personal Finance Tracer Content**: Personal dashboard, transaction logging, smart categorization, budget tracking, charts & visualizations. |

## 🌟 Key Features

- **Personal Finance Tracking**: Monthly expense categorization, visual cash flow breakdowns, dynamic budget health scoring.
- **Corporate Office Finance**: B2B revenue and expense ledgers, operational runway forecasting, net margin calculations.
- **Administration Portal**: Platform treasury overview, runtime operating expense logging, Office license verification.
- **Security & Multi-Tenant**: Role-based access, strict data isolation, PBKDF2 password cryptography.
- **Modern UI**: Full Dark/Light theme switching, vector geometry icons, smooth animated transitions, non-blocking toast notifications.

## 🛠️ Technology Stack

- **Framework**: .NET 8 (WPF - Windows Presentation Foundation)
- **Architecture**: MVVM (Model-View-ViewModel) with RelayCommand pattern
- **Database**: SQLite with Entity Framework Core
- **Testing**: xUnit with isolated in-memory/test databases
