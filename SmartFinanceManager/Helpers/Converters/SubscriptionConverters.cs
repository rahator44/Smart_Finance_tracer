using SmartFinanceManager.Services;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;

namespace SmartFinanceManager.Helpers
{
    public class SubscriptionStatusToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush GreenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"));
        private static readonly SolidColorBrush OrangeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9500"));
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3B30"));
        private static readonly SolidColorBrush MutedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8E8E93"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SubscriptionStatus status)
            {
                return status switch
                {
                    SubscriptionStatus.Approved => GreenBrush,
                    SubscriptionStatus.Pending => OrangeBrush,
                    SubscriptionStatus.Rejected => RedBrush,
                    _ => MutedBrush
                };
            }
            if (value is string s && Enum.TryParse<SubscriptionStatus>(s, true, out var parsed))
            {
                return parsed switch
                {
                    SubscriptionStatus.Approved => GreenBrush,
                    SubscriptionStatus.Pending => OrangeBrush,
                    SubscriptionStatus.Rejected => RedBrush,
                    _ => MutedBrush
                };
            }
            return MutedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SubscriptionStatusToBadgeBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLight = ThemeService.IsLightTheme;
            if (value is SubscriptionStatus status)
            {
                if (isLight)
                {
                    return status switch
                    {
                        SubscriptionStatus.Approved => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7")),
                        SubscriptionStatus.Pending => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")),
                        SubscriptionStatus.Rejected => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"))
                    };
                }
                else
                {
                    return status switch
                    {
                        SubscriptionStatus.Approved => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252ECC71")),
                        SubscriptionStatus.Pending => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25FF9500")),
                        SubscriptionStatus.Rejected => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25FF3B30")),
                        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252528"))
                    };
                }
            }
            return isLight 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252528"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SubscriptionStatusToPendingVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SubscriptionStatus status)
            {
                return status == SubscriptionStatus.Pending ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            if (value is string s && Enum.TryParse<SubscriptionStatus>(s, true, out var parsed))
            {
                return parsed == SubscriptionStatus.Pending ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SubscriptionStatusToResolvedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SubscriptionStatus status)
            {
                return status != SubscriptionStatus.Pending ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            if (value is string s && Enum.TryParse<SubscriptionStatus>(s, true, out var parsed))
            {
                return parsed != SubscriptionStatus.Pending ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class UserToRoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string email && AdminConfig.IsAdmin(email))
            {
                return "ADMIN";
            }
            if (value is User user && AdminConfig.IsAdmin(user))
            {
                return "ADMIN";
            }
            return "USER";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class UserRoleToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush AdminBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3B30"));
        private static readonly SolidColorBrush UserBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string email && AdminConfig.IsAdmin(email))
            {
                return AdminBrush;
            }
            if (value is User user && AdminConfig.IsAdmin(user))
            {
                return AdminBrush;
            }
            return UserBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class UserRoleToBadgeBgConverter : IValueConverter
    {
        private static readonly SolidColorBrush AdminBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
        private static readonly SolidColorBrush UserBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string email && AdminConfig.IsAdmin(email))
            {
                return AdminBg;
            }
            if (value is User user && AdminConfig.IsAdmin(user))
            {
                return AdminBg;
            }
            return UserBg;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
