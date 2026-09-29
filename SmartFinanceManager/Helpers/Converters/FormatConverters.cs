using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.Helpers
{
    public class AmountToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
        private static readonly SolidColorBrush GreenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal num = 0;
            if (value is decimal d) num = d;
            else if (value is double dbl) num = (decimal)dbl;
            else if (value is int i) num = i;
            else if (value is float f) num = (decimal)f;
            else if (value is long l) num = l;
            else if (value != null && decimal.TryParse(value.ToString(), out decimal parsed)) num = parsed;

            if (num < 0) return RedBrush;
            if (num > 0) return GreenBrush;
            return Application.Current?.TryFindResource("TextMutedBrush") as Brush 
                   ?? (ThemeService.IsLightTheme 
                       ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B")) 
                       : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8E8E93")));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class AmountFormatterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal num = 0;
            if (value is decimal d) num = d;
            else if (value is double dbl) num = (decimal)dbl;
            else if (value is int i) num = i;
            else if (value is float f) num = (decimal)f;
            else if (value is long l) num = l;
            else if (value != null && decimal.TryParse(value.ToString(), out decimal parsed)) num = parsed;

            if (num > 0) return $"+${num:N2}";
            if (num < 0) return $"-${Math.Abs(num):N2}";
            return "$0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CurrencyAbsFormatterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal num = 0;
            if (value is decimal d) num = d;
            else if (value is double dbl) num = (decimal)dbl;
            else if (value is int i) num = i;
            else if (value is float f) num = (decimal)f;
            else if (value is long l) num = l;
            else if (value != null && decimal.TryParse(value.ToString(), out decimal parsed)) num = parsed;

            return $"${Math.Abs(num):N2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TrendPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double pct)
            {
                if (pct > 0) return $"+{pct:F1}% vs prev mo";
                if (pct < 0) return $"{pct:F1}% vs prev mo";
                return "0.0% vs prev mo";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
