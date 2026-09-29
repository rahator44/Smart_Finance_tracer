using SmartFinanceManager.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartFinanceManager.Helpers
{
    public class StringToGeometryConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                if (Application.Current != null && Application.Current.TryFindResource(str) is Geometry directGeo)
                {
                    return directGeo;
                }

                string key = MapCategoryToIconKey(str);
                if (Application.Current != null && Application.Current.TryFindResource(key) is Geometry mappedGeo)
                {
                    return mappedGeo;
                }
            }

            if (Application.Current != null && Application.Current.TryFindResource("IconOthers") is Geometry fallback)
            {
                return fallback;
            }
            return null;
        }

        private static string MapCategoryToIconKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "IconOthers";
            if (name.Contains("Food") || name.Contains("Meal") || name.Contains("Dining")) return "IconFood";
            if (name.Contains("Transport") || name.Contains("Travel") || name.Contains("Car")) return "IconTransportation";
            if (name.Contains("Shop") || name.Contains("Grocer")) return "IconShopping";
            if (name.Contains("Entertain") || name.Contains("Movie")) return "IconEntertainment";
            if (name.Contains("Housing") || name.Contains("Rent") || name.Contains("Lease")) return "IconHousing";
            if (name.Contains("Phone") || name.Contains("Internet") || name.Contains("Fiber")) return "IconPhone";
            if (name.Contains("Util") || name.Contains("Electricity") || name.Contains("Water") || name.Contains("Bill")) return "IconUtilities";
            if (name.Contains("Health") || name.Contains("Medical") || name.Contains("Doctor")) return "IconHealth";
            if (name.Contains("Education") || name.Contains("Course") || name.Contains("School")) return "IconEducation";
            if (name.Contains("Cloth") || name.Contains("Apparel")) return "IconClothing";
            if (name.Contains("Sport") || name.Contains("Gym") || name.Contains("Fitness")) return "IconSports";
            if (name.Contains("Salary") || name.Contains("Payroll") || name.Contains("Wage")) return "IconSalary";
            if (name.Contains("Bonus")) return "IconBonus";
            if (name.Contains("Part-Time") || name.Contains("Freelance") || name.Contains("Consult")) return "IconPartTime";
            if (name.Contains("Invest") || name.Contains("Saving") || name.Contains("Emergency") || name.Contains("Treasury")) return "IconInvestment";
            if (name.Contains("Gift") || name.Contains("Grant")) return "IconGifts";
            if (name.Contains("Software") || name.Contains("SaaS") || name.Contains("Cloud")) return "IconElectronics";
            if (name.Contains("Market") || name.Contains("Ad")) return "IconMarketing";
            if (name.Contains("Hard") || name.Contains("Server") || name.Contains("Equip")) return "IconEquipment";
            if (name.Contains("Maint") || name.Contains("Repair")) return "IconMaintenance";
            if (name.Contains("Supply") || name.Contains("Pantry") || name.Contains("Office")) return "IconBeauty";
            if (name.Contains("Client") || name.Contains("Social") || name.Contains("Hospit")) return "IconSocial";
            if (name.Contains("Sales") || name.Contains("Revenue") || name.Contains("Capital")) return "IconSales";
            if (name.Contains("Tax") || name.Contains("Legal")) return "IconTax";
            return "IconOthers";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CategorySelectionToCircleBrushConverter : IMultiValueConverter
    {
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3B30"));
        private static readonly SolidColorBrush BlueBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF"));
        private static readonly SolidColorBrush DarkCircleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C2C32"));
        private static readonly SolidColorBrush LightCircleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDark = values != null && values.Length >= 4 && values[3] is bool b ? b : ThemeService.IsDarkTheme;

            if (values != null && values.Length >= 2 && values[0] != null && values[1] != null)
            {
                string selectedCat = values[0].ToString() ?? string.Empty;
                string itemCat = values[1].ToString() ?? string.Empty;
                string txType = values.Length >= 3 && values[2] != null ? values[2].ToString() ?? "Expense" : "Expense";

                if (!string.IsNullOrWhiteSpace(selectedCat) && selectedCat.Equals(itemCat, StringComparison.OrdinalIgnoreCase))
                {
                    // When clicked/selected: turns Red for expense, Blue for income/transfer
                    return txType.Equals("Expense", StringComparison.OrdinalIgnoreCase) ? RedBrush : BlueBrush;
                }
            }
            return isDark ? DarkCircleBrush : LightCircleBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CategoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category)
            {
                if (category.Contains("Sales") || category.Contains("Salary") || category.Contains("Payroll")) return "💼";
                if (category.Contains("Bonus") || category.Contains("Revenue")) return "💰";
                if (category.Contains("Food") || category.Contains("Meal") || category.Contains("Grocer")) return "🍔";
                if (category.Contains("Part-Time") || category.Contains("Invest") || category.Contains("Consulting")) return "📈";
                if (category.Contains("Shop") || category.Contains("Supplier")) return "🛍️";
                if (category.Contains("Entertain") || category.Contains("Equip") || category.Contains("Software")) return "🎬";
                if (category.Contains("Rent") || category.Contains("Lease") || category.Contains("Hous")) return "🏠";
                if (category.Contains("Util") || category.Contains("Phone") || category.Contains("Bill")) return "⚡";
                if (category.Contains("Transport") || category.Contains("Car") || category.Contains("Travel")) return "🚗";
                if (category.Contains("Market") || category.Contains("Ad")) return "📢";
                if (category.Contains("Tax") || category.Contains("Legal")) return "⚖️";
            }
            return "📝";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CategoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category)
            {
                string hexColor = "#9E9E9E"; // Default Gray

                if (category.Contains("Sales") || category.Contains("Salary") || category.Contains("Payroll")) hexColor = "#007AFF"; // Blue
                else if (category.Contains("Bonus") || category.Contains("Revenue")) hexColor = "#2ECC71"; // Emerald Green
                else if (category.Contains("Food") || category.Contains("Meal") || category.Contains("Grocer")) hexColor = "#E74C3C"; // Red (Expense food)
                else if (category.Contains("Part-Time") || category.Contains("Invest") || category.Contains("Consulting") || category.Contains("Saving") || category.Contains("Emergency") || category.Contains("Treasury")) hexColor = "#007AFF"; // Blue
                else if (category.Contains("Shop") || category.Contains("Supplier") || category.Contains("Tax")) hexColor = "#E74C3C"; // Red
                else if (category.Contains("Entertain") || category.Contains("Equip") || category.Contains("Software")) hexColor = "#9B59B6"; // Purple
                else if (category.Contains("Rent") || category.Contains("Lease") || category.Contains("Hous")) hexColor = "#E67E22"; // Orange
                else if (category.Contains("Util") || category.Contains("Phone") || category.Contains("Bill")) hexColor = "#34495E"; // Dark Gray
                else if (category.Contains("Transport") || category.Contains("Car") || category.Contains("Travel")) hexColor = "#E74C3C"; // Red
                else if (category.Contains("Market") || category.Contains("Ad")) hexColor = "#1ABC9C"; // Turquoise

                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CategorySelectedToBorderBrushConverter : IMultiValueConverter
    {
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3B30"));
        private static readonly SolidColorBrush BlueBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF"));
        private static readonly SolidColorBrush DarkBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C2C2E"));
        private static readonly SolidColorBrush LightBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDark = values != null && values.Length >= 4 && values[3] is bool b ? b : ThemeService.IsDarkTheme;

            if (values != null && values.Length >= 2 && values[0] != null && values[1] != null)
            {
                string selectedCat = values[0].ToString() ?? string.Empty;
                string itemCat = values[1].ToString() ?? string.Empty;
                string txType = values.Length >= 3 && values[2] != null ? values[2].ToString() ?? "Expense" : "Expense";

                if (!string.IsNullOrWhiteSpace(selectedCat) && selectedCat.Equals(itemCat, StringComparison.OrdinalIgnoreCase))
                {
                    return txType.Equals("Expense", StringComparison.OrdinalIgnoreCase) ? RedBrush : BlueBrush;
                }
            }
            return isDark ? DarkBorderBrush : LightBorderBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CategorySelectedToBackgroundConverter : IMultiValueConverter
    {
        private static readonly SolidColorBrush LightSelectedExpenseBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
        private static readonly SolidColorBrush LightSelectedIncomeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
        private static readonly SolidColorBrush DarkSelectedExpenseBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25FF3B30"));
        private static readonly SolidColorBrush DarkSelectedIncomeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25007AFF"));
        private static readonly SolidColorBrush LightUnselectedBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
        private static readonly SolidColorBrush DarkUnselectedBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C1C1E"));

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDark = values != null && values.Length >= 4 && values[3] is bool b ? b : ThemeService.IsDarkTheme;

            if (values != null && values.Length >= 2 && values[0] != null && values[1] != null)
            {
                string selectedCat = values[0].ToString() ?? string.Empty;
                string itemCat = values[1].ToString() ?? string.Empty;
                string txType = values.Length >= 3 && values[2] != null ? values[2].ToString() ?? "Expense" : "Expense";

                if (!string.IsNullOrWhiteSpace(selectedCat) && selectedCat.Equals(itemCat, StringComparison.OrdinalIgnoreCase))
                {
                    bool isExpense = txType.Equals("Expense", StringComparison.OrdinalIgnoreCase);
                    if (!isDark)
                    {
                        return isExpense ? LightSelectedExpenseBg : LightSelectedIncomeBg;
                    }
                    else
                    {
                        return isExpense ? DarkSelectedExpenseBg : DarkSelectedIncomeBg;
                    }
                }
            }
            return isDark ? DarkUnselectedBg : LightUnselectedBg;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TransactionTypeToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3B30"));
        private static readonly SolidColorBrush GreenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"));
        private static readonly SolidColorBrush BlueBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string txType)
            {
                if (txType.Equals("Expense", StringComparison.OrdinalIgnoreCase) || txType.Equals("Expenses", StringComparison.OrdinalIgnoreCase))
                    return RedBrush;
                if (txType.Equals("Income", StringComparison.OrdinalIgnoreCase))
                    return GreenBrush;
                if (txType.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
                    return BlueBrush;
            }
            if (value is bool isExpense)
            {
                return isExpense ? RedBrush : BlueBrush;
            }
            return BlueBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TransactionTypeToBadgeBgConverter : IValueConverter
    {
        private static readonly SolidColorBrush RedBadgeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
        private static readonly SolidColorBrush GreenBadgeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));
        private static readonly SolidColorBrush BlueBadgeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string txType)
            {
                if (txType.Equals("Expense", StringComparison.OrdinalIgnoreCase))
                    return RedBadgeBg;
                if (txType.Equals("Income", StringComparison.OrdinalIgnoreCase))
                    return GreenBadgeBg;
                if (txType.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
                    return BlueBadgeBg;
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F6"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
