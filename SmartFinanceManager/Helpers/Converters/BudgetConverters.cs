using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.Helpers
{
    public class PercentageToDashArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double percentage = 0;
            if (value is double d) percentage = d;
            else if (value is decimal dec) percentage = (double)dec;
            else if (value is int i) percentage = i;
            else if (value != null && double.TryParse(value.ToString(), out double parsed)) percentage = parsed;

            if (percentage < 0) percentage = 0;
            if (percentage > 100) percentage = 100;

            double diameter = 190;
            double strokeThickness = 16;

            if (parameter is string paramStr)
            {
                var parts = paramStr.Split(',');
                if (parts.Length >= 1) double.TryParse(parts[0], out diameter);
                if (parts.Length >= 2) double.TryParse(parts[1], out strokeThickness);
            }

            double circumference = Math.PI * diameter;
            double strokeUnits = circumference / strokeThickness;

            double dash = (percentage / 100.0) * strokeUnits;
            double gap = strokeUnits + 10;

            return new DoubleCollection { Math.Max(0.001, dash), gap };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BudgetHealthToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BudgetHealth health)
            {
                return health switch
                {
                    BudgetHealth.Healthy => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                    BudgetHealth.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                    BudgetHealth.Exceeded => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF"))
                };
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BudgetHealthToBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLight = ThemeService.IsLightTheme;
            if (value is BudgetHealth health)
            {
                if (isLight)
                {
                    return health switch
                    {
                        BudgetHealth.Healthy => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7")),
                        BudgetHealth.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")),
                        BudgetHealth.Exceeded => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"))
                    };
                }
                else
                {
                    return health switch
                    {
                        BudgetHealth.Healthy => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252ECC71")),
                        BudgetHealth.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25FF9800")),
                        BudgetHealth.Exceeded => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25FF3B30")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20252528"))
                    };
                }
            }
            return isLight
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20252528"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BudgetHealthToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLight = ThemeService.IsLightTheme;
            if (value is BudgetHealth health)
            {
                if (isLight)
                {
                    return health switch
                    {
                        BudgetHealth.Healthy => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534")),
                        BudgetHealth.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E")),
                        BudgetHealth.Exceeded => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"))
                    };
                }
                else
                {
                    return health switch
                    {
                        BudgetHealth.Healthy => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
                        BudgetHealth.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB74D")),
                        BudgetHealth.Exceeded => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"))
                    };
                }
            }
            return isLight
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
