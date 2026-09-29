using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class CategoryItem
    {
        public string Name { get; set; } = string.Empty;
        public string IconTag { get; set; } = string.Empty;
    }

    public class TransactionTemplate
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = "Expense";
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class AddTransactionViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TransactionService _transactionService;
        private readonly Transaction? _editingTransaction;

        private string _transactionType = "Expense"; // "Expense", "Income", "Transfer"
        private string _selectedCategory = string.Empty;
        private decimal _amount;
        private string _description = string.Empty;
        private DateTime _date = DateTime.Today;
        private string _errorMessage = string.Empty;
        private string _smartSuggestion = string.Empty;
        private ObservableCollection<CategoryItem> _categories = new();
        private TransactionTemplate? _selectedPresetTemplate;
        public ObservableCollection<TransactionTemplate> PresetTemplates { get; } = new();

        public TransactionTemplate? SelectedPresetTemplate
        {
            get => _selectedPresetTemplate;
            set
            {
                if (SetProperty(ref _selectedPresetTemplate, value) && value != null)
                {
                    TransactionType = value.Type;
                    SelectedCategory = value.Category;
                    Amount = value.Amount;
                    Description = value.Description;
                }
            }
        }

        public bool IsOffice { get; }

        public bool IsEditMode => _editingTransaction != null;
        public string TitleText => IsEditMode 
            ? (IsOffice ? "Edit Office Transaction" : "Edit Personal Transaction") 
            : (IsOffice ? "Add Office Transaction" : "Add Personal Transaction");

        public string ExpenseTabHeader => IsOffice ? "Office Expense" : "Expense";
        public string IncomeTabHeader => IsOffice ? "Office Revenue" : "Income";
        public string TransferTabHeader => IsOffice ? "Corporate Transfer" : "Transfer";

        public bool IsExpenseType
        {
            get => TransactionType == "Expense";
            set { if (value) TransactionType = "Expense"; }
        }

        public bool IsIncomeType
        {
            get => TransactionType == "Income";
            set { if (value) TransactionType = "Income"; }
        }

        public bool IsTransferType
        {
            get => TransactionType == "Transfer";
            set { if (value) TransactionType = "Transfer"; }
        }

        public string TransactionType
        {
            get => _transactionType;
            set
            {
                if (SetProperty(ref _transactionType, value))
                {
                    OnPropertyChanged(nameof(IsExpenseType));
                    OnPropertyChanged(nameof(IsIncomeType));
                    OnPropertyChanged(nameof(IsTransferType));
                    UpdateCategories();
                }
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    CheckSmartAutoCategorization();
                }
            }
        }

        public string SmartSuggestion
        {
            get => _smartSuggestion;
            set => SetProperty(ref _smartSuggestion, value);
        }

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<CategoryItem> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ICommand SelectCategoryCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveAsPresetCommand { get; }

        public AddTransactionViewModel(MainViewModel mainViewModel, TransactionService transactionService, bool isOffice, Transaction? editingTransaction = null)
        {
            _mainViewModel = mainViewModel;
            _transactionService = transactionService;
            IsOffice = isOffice;
            _editingTransaction = editingTransaction;

            SelectCategoryCommand = new RelayCommand(ExecuteSelectCategory);
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
            SaveAsPresetCommand = new RelayCommand(ExecuteSaveAsPreset);

            InitializePresets();
            UpdateCategories();

            if (_editingTransaction != null)
            {
                TransactionType = _editingTransaction.Type;
                SelectedCategory = _editingTransaction.Category;
                Amount = Math.Abs(_editingTransaction.Amount);
                Description = _editingTransaction.Description;
                Date = _editingTransaction.Date;
            }
            else
            {
                // Default category
                SelectedCategory = Categories.FirstOrDefault()?.Name ?? string.Empty;
            }
        }

        private void CheckSmartAutoCategorization()
        {
            if (IsEditMode) return;

            var prediction = SmartCategorizationService.Predict(Description, IsOffice);
            if (prediction.IsConfident)
            {
                if (TransactionType != prediction.Type)
                {
                    TransactionType = prediction.Type;
                }
                SelectedCategory = prediction.Category;
                SmartSuggestion = $"Smart Categorized: '{prediction.Category}' ({prediction.Type})";
            }
            else
            {
                SmartSuggestion = string.Empty;
            }
        }

        private void UpdateCategories()
        {
            Categories.Clear();

            if (!IsOffice)
            {
                if (TransactionType == "Expense")
                {
                    Categories.Add(new CategoryItem { Name = "Food & Dining", IconTag = "IconFood" });
                    Categories.Add(new CategoryItem { Name = "Transportation", IconTag = "IconTransportation" });
                    Categories.Add(new CategoryItem { Name = "Shopping", IconTag = "IconShopping" });
                    Categories.Add(new CategoryItem { Name = "Entertainment", IconTag = "IconEntertainment" });
                    Categories.Add(new CategoryItem { Name = "Housing", IconTag = "IconHousing" });
                    Categories.Add(new CategoryItem { Name = "Phone & Internet", IconTag = "IconPhone" });
                    Categories.Add(new CategoryItem { Name = "Utilities", IconTag = "IconUtilities" });
                    Categories.Add(new CategoryItem { Name = "Health & Medical", IconTag = "IconHealth" });
                    Categories.Add(new CategoryItem { Name = "Education", IconTag = "IconEducation" });
                    Categories.Add(new CategoryItem { Name = "Clothing", IconTag = "IconClothing" });
                    Categories.Add(new CategoryItem { Name = "Sports & Fitness", IconTag = "IconSports" });
                    Categories.Add(new CategoryItem { Name = "Travel", IconTag = "IconTravel" });
                    Categories.Add(new CategoryItem { Name = "Other Expense", IconTag = "IconOthers" });
                }
                else if (TransactionType == "Income")
                {
                    Categories.Add(new CategoryItem { Name = "Salary & Wages", IconTag = "IconSalary" });
                    Categories.Add(new CategoryItem { Name = "Bonus", IconTag = "IconBonus" });
                    Categories.Add(new CategoryItem { Name = "Freelance & Consulting", IconTag = "IconPartTime" });
                    Categories.Add(new CategoryItem { Name = "Investment Returns", IconTag = "IconInvestment" });
                    Categories.Add(new CategoryItem { Name = "Gifts & Grants", IconTag = "IconGifts" });
                    Categories.Add(new CategoryItem { Name = "Other Income", IconTag = "IconOthers" });
                }
                else // Transfer
                {
                    Categories.Add(new CategoryItem { Name = "Investments", IconTag = "IconInvestment" });
                    Categories.Add(new CategoryItem { Name = "Savings Account", IconTag = "IconOthers" });
                    Categories.Add(new CategoryItem { Name = "Emergency Fund", IconTag = "IconHealth" });
                }
            }
            else // Office
            {
                if (TransactionType == "Expense")
                {
                    Categories.Add(new CategoryItem { Name = "Staff Salaries & Payroll", IconTag = "IconSalary" });
                    Categories.Add(new CategoryItem { Name = "Office Rent & Lease", IconTag = "IconRent" });
                    Categories.Add(new CategoryItem { Name = "Software & SaaS", IconTag = "IconElectronics" });
                    Categories.Add(new CategoryItem { Name = "Marketing & Ads", IconTag = "IconMarketing" });
                    Categories.Add(new CategoryItem { Name = "Hardware & Servers", IconTag = "IconEquipment" });
                    Categories.Add(new CategoryItem { Name = "Utilities & Fiber", IconTag = "IconUtilities" });
                    Categories.Add(new CategoryItem { Name = "Tax & Legal Fees", IconTag = "IconTax" });
                    Categories.Add(new CategoryItem { Name = "Consulting & Audit", IconTag = "IconEducation" });
                    Categories.Add(new CategoryItem { Name = "Office Maintenance", IconTag = "IconMaintenance" });
                    Categories.Add(new CategoryItem { Name = "Pantry & Supplies", IconTag = "IconBeauty" });
                    Categories.Add(new CategoryItem { Name = "Client Hospitality", IconTag = "IconSocial" });
                    Categories.Add(new CategoryItem { Name = "Other Office Expense", IconTag = "IconOthers" });
                }
                else if (TransactionType == "Income")
                {
                    Categories.Add(new CategoryItem { Name = "Client Sales", IconTag = "IconSales" });
                    Categories.Add(new CategoryItem { Name = "Consulting Fees", IconTag = "IconSalary" });
                    Categories.Add(new CategoryItem { Name = "Service Revenue", IconTag = "IconBonus" });
                    Categories.Add(new CategoryItem { Name = "Enterprise Contracts", IconTag = "IconPartTime" });
                    Categories.Add(new CategoryItem { Name = "Software Licensing", IconTag = "IconElectronics" });
                    Categories.Add(new CategoryItem { Name = "Other Corporate Income", IconTag = "IconOthers" });
                }
                else // Corporate Transfer
                {
                    Categories.Add(new CategoryItem { Name = "Treasury", IconTag = "IconInvestment" });
                    Categories.Add(new CategoryItem { Name = "Capital Injection", IconTag = "IconSales" });
                    Categories.Add(new CategoryItem { Name = "Intercompany Wire", IconTag = "IconOthers" });
                }
            }

            if (!Categories.Any(c => c.Name == SelectedCategory))
            {
                SelectedCategory = Categories.FirstOrDefault()?.Name ?? string.Empty;
            }
        }

        private void ExecuteSelectCategory(object? parameter)
        {
            if (parameter is string categoryName)
            {
                SelectedCategory = categoryName;
            }
        }

        private void ExecuteSave()
        {
            ErrorMessage = string.Empty;

            if (Amount <= 0)
            {
                ErrorMessage = "Please enter a valid amount greater than $0.";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                ErrorMessage = "Please select a category.";
                return;
            }

            if (AuthService.CurrentUser == null) return;

            int userId = AuthService.CurrentUser.Id;

            // Expenses and Personal transfers are stored as negative numbers for liquid balance math
            decimal signedAmount;
            if (TransactionType == "Expense")
            {
                signedAmount = -Math.Abs(Amount);
            }
            else if (TransactionType == "Transfer")
            {
                signedAmount = IsOffice ? Math.Abs(Amount) : -Math.Abs(Amount);
            }
            else
            {
                signedAmount = Math.Abs(Amount);
            }

            if (IsEditMode && _editingTransaction != null)
            {
                _editingTransaction.Type = TransactionType;
                _editingTransaction.Category = SelectedCategory;
                _editingTransaction.Amount = signedAmount;
                _editingTransaction.Date = Date;
                _editingTransaction.Description = Description?.Trim() ?? string.Empty;
                _editingTransaction.IsOffice = IsOffice;

                bool updated = _transactionService.UpdateTransaction(_editingTransaction, userId);
                if (updated)
                {
                    _mainViewModel.ShowToast($"Updated {SelectedCategory} transaction (${Amount:N0}).");
                }
                else
                {
                    ErrorMessage = "Unable to update transaction. Record ownership mismatch.";
                    return;
                }
            }
            else
            {
                var newTx = new Transaction
                {
                    UserId = userId,
                    Type = TransactionType,
                    Category = SelectedCategory,
                    Amount = signedAmount,
                    Date = Date,
                    IsOffice = IsOffice,
                    Description = Description?.Trim() ?? string.Empty
                };

                _transactionService.AddTransaction(newTx);
                _mainViewModel.ShowToast($"Added {TransactionType} of ${Amount:N0} to {SelectedCategory}.");
            }

            // Return to Dashboard and display the month of the added/edited transaction
            _mainViewModel.NavigateHome(new DateTime(Date.Year, Date.Month, 1));
        }

        private void ExecuteCancel()
        {
            _mainViewModel.NavigateHome();
        }

        private void InitializePresets()
        {
            PresetTemplates.Clear();
            if (!IsOffice)
            {
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "Monthly Salary ($4,500)",
                    Type = "Income",
                    Category = "Salary & Wages",
                    Amount = 4500,
                    Description = "Primary employment monthly salary"
                });
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "Supermarket Groceries (-$450)",
                    Type = "Expense",
                    Category = "Food & Dining",
                    Amount = 450,
                    Description = "Weekly grocery store trip"
                });
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "Apartment Rent (-$1,200)",
                    Type = "Expense",
                    Category = "Housing",
                    Amount = 1200,
                    Description = "Monthly apartment rent"
                });
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "High-Yield Savings Transfer (-$500)",
                    Type = "Transfer",
                    Category = "Investments",
                    Amount = 500,
                    Description = "Monthly savings deposit"
                });
            }
            else
            {
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "Enterprise License Revenue ($22,000)",
                    Type = "Income",
                    Category = "Client Sales",
                    Amount = 22000,
                    Description = "Enterprise license contract payment"
                });
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "Team Payroll (-$8,500)",
                    Type = "Expense",
                    Category = "Staff Salaries & Payroll",
                    Amount = 8500,
                    Description = "Monthly team compensation & payroll"
                });
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "Office Commercial Lease (-$3,200)",
                    Type = "Expense",
                    Category = "Office Rent & Lease",
                    Amount = 3200,
                    Description = "Commercial headquarters space lease"
                });
                PresetTemplates.Add(new TransactionTemplate
                {
                    DisplayName = "Cloud Infrastructure AWS (-$1,400)",
                    Type = "Expense",
                    Category = "Software & SaaS",
                    Amount = 1400,
                    Description = "Monthly cloud server hosting"
                });
            }
        }

        private void ExecuteSaveAsPreset()
        {
            if (Amount <= 0 || string.IsNullOrWhiteSpace(SelectedCategory))
            {
                ErrorMessage = "Please specify a category and amount first.";
                return;
            }

            string display = $"{SelectedCategory} ({(TransactionType == "Expense" ? "-" : "")}${Amount:N0})";
            var newPreset = new TransactionTemplate
            {
                DisplayName = display,
                Type = TransactionType,
                Category = SelectedCategory,
                Amount = Amount,
                Description = Description
            };

            PresetTemplates.Add(newPreset);
            SelectedPresetTemplate = newPreset;
            _mainViewModel.ShowToast($"Saved preset '{display}'!");
        }
    }
}
