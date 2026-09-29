using System;
using System.IO;
#if WINDOWS
using System.Windows;
using System.Windows.Media;
#endif

namespace SmartFinanceManager.Services
{
    public enum AppTheme
    {
        Dark,
        Light
    }

    public static class ThemeService
    {
        private static readonly string ThemePrefFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.pref");
        
        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;
        
        public static bool IsDarkTheme => CurrentTheme == AppTheme.Dark;
        public static bool IsLightTheme => CurrentTheme == AppTheme.Light;

        public static event Action<AppTheme>? ThemeChanged;

        public static void Initialize()
        {
            try
            {
                if (File.Exists(ThemePrefFilePath))
                {
                    string saved = File.ReadAllText(ThemePrefFilePath).Trim();
                    if (Enum.TryParse<AppTheme>(saved, true, out var savedTheme))
                    {
                        ApplyTheme(savedTheme, saveToDisk: false);
                        return;
                    }
                }
            }
            catch { }

            // Default to Dark theme
            ApplyTheme(AppTheme.Dark, saveToDisk: false);
        }

        public static void SetTheme(AppTheme theme)
        {
            ApplyTheme(theme, saveToDisk: true);
        }

        public static void ToggleTheme()
        {
            SetTheme(CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark);
        }

        private static void ApplyTheme(AppTheme theme, bool saveToDisk)
        {
            CurrentTheme = theme;

            if (saveToDisk)
            {
                try
                {
                    File.WriteAllText(ThemePrefFilePath, theme.ToString());
                }
                catch { }
            }

#if WINDOWS
            if (Application.Current != null)
            {
                ApplyPalette(theme);
            }
#endif

            ThemeChanged?.Invoke(theme);
        }

#if WINDOWS
        private static void ApplyPalette(AppTheme theme)
        {
            var res = Application.Current?.Resources;
            if (res == null) return;

            void SetRes(string key, SolidColorBrush brush)
            {
                res[key] = brush;
                foreach (ResourceDictionary md in res.MergedDictionaries)
                {
                    if (md.Contains(key))
                    {
                        md[key] = brush;
                    }
                }
            }

            if (theme == AppTheme.Light)
            {
                // Modern Light Palette (Stripe/Apple/Linear Clean Light Design)
                SetRes("BackgroundBrush", BrushFromHex("#F8FAFC"));         // Slate 50 clean backdrop
                SetRes("NavigationBrush", BrushFromHex("#FFFFFF"));         // Pure white header/footer
                SetRes("CardBrush", BrushFromHex("#FFFFFF"));               // Pure white card surfaces
                SetRes("CardSecondaryBrush", BrushFromHex("#F1F5F9"));      // Slate 100 secondary container
                SetRes("InputBackgroundBrush", BrushFromHex("#FFFFFF"));    // Clean white inputs
                SetRes("TextBrush", BrushFromHex("#0F172A"));               // High-contrast slate 900
                SetRes("TextMutedBrush", BrushFromHex("#64748B"));          // Slate 500 readable muted
                SetRes("TextSubtleBrush", BrushFromHex("#94A3B8"));         // Slate 400 subtle
                SetRes("BorderBrush", BrushFromHex("#E2E8F0"));             // Slate 200 crisp borders
                SetRes("BorderFocusBrush", BrushFromHex("#007AFF"));
                SetRes("AccentBrush", BrushFromHex("#007AFF"));             // Vibrant blue
                SetRes("AccentMouseOverBrush", BrushFromHex("#0066D6"));
                SetRes("AccentSelectedBrush", BrushFromHex("#0051AB"));
                SetRes("AccentGlowBrush", BrushFromHex("#20007AFF"));
                SetRes("RedBrush", BrushFromHex("#EF4444"));                // Modern Tailwind red
                SetRes("RedHoverBrush", BrushFromHex("#DC2626"));
                SetRes("RedPressedBrush", BrushFromHex("#B91C1C"));
                SetRes("RedBadgeBgBrush", BrushFromHex("#FEE2E2"));
                SetRes("GreenBrush", BrushFromHex("#10B981"));              // Emerald
                SetRes("GreenBadgeBgBrush", BrushFromHex("#DCFCE7"));
                SetRes("OrangeBrush", BrushFromHex("#F59E0B"));             // Amber
                SetRes("OrangeBadgeBgBrush", BrushFromHex("#FEF3C7"));
                SetRes("PopupBackgroundBrush", BrushFromHex("#FFFFFF"));
                SetRes("HoverBackgroundBrush", BrushFromHex("#E2E8F0"));
                SetRes("SelectedBackgroundBrush", BrushFromHex("#E2E8F0"));
            }
            else
            {
                // Sleek Dark Palette
                SetRes("BackgroundBrush", BrushFromHex("#121212"));
                SetRes("NavigationBrush", BrushFromHex("#0A0A0A"));
                SetRes("CardBrush", BrushFromHex("#1C1C1E"));
                SetRes("CardSecondaryBrush", BrushFromHex("#252528"));
                SetRes("InputBackgroundBrush", BrushFromHex("#252528"));
                SetRes("TextBrush", BrushFromHex("#FFFFFF"));
                SetRes("TextMutedBrush", BrushFromHex("#8E8E93"));
                SetRes("TextSubtleBrush", BrushFromHex("#636366"));
                SetRes("BorderBrush", BrushFromHex("#2C2C2E"));
                SetRes("BorderFocusBrush", BrushFromHex("#007AFF"));
                SetRes("AccentBrush", BrushFromHex("#007AFF"));
                SetRes("AccentMouseOverBrush", BrushFromHex("#3395FF"));
                SetRes("AccentSelectedBrush", BrushFromHex("#1A85FF"));
                SetRes("AccentGlowBrush", BrushFromHex("#33007AFF"));
                SetRes("RedBrush", BrushFromHex("#FF3B30"));
                SetRes("RedHoverBrush", BrushFromHex("#FF5347"));
                SetRes("RedPressedBrush", BrushFromHex("#D32F2F"));
                SetRes("RedBadgeBgBrush", BrushFromHex("#25FF3B30"));
                SetRes("GreenBrush", BrushFromHex("#2ECC71"));
                SetRes("GreenBadgeBgBrush", BrushFromHex("#252ECC71"));
                SetRes("OrangeBrush", BrushFromHex("#FF9800"));
                SetRes("OrangeBadgeBgBrush", BrushFromHex("#25FF9800"));
                SetRes("PopupBackgroundBrush", BrushFromHex("#1C1C20"));
                SetRes("HoverBackgroundBrush", BrushFromHex("#2C2C32"));
                SetRes("SelectedBackgroundBrush", BrushFromHex("#2C2C30"));
            }
        }

        private static SolidColorBrush BrushFromHex(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }
#endif
    }
}
