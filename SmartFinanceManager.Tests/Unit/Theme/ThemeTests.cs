using System;
using System.IO;
using Xunit;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.Tests
{
    public class ThemeTests
    {
        [Fact]
        public void ThemeService_InitializesToDefaultOrSaved()
        {
            // Reset to dark
            ThemeService.SetTheme(AppTheme.Dark);
            Assert.True(ThemeService.IsDarkTheme);
            Assert.False(ThemeService.IsLightTheme);
            Assert.Equal(AppTheme.Dark, ThemeService.CurrentTheme);
        }

        [Fact]
        public void ThemeService_SetTheme_UpdatesStateAndRaisesEvent()
        {
            AppTheme? receivedTheme = null;
            Action<AppTheme> handler = t => receivedTheme = t;
            ThemeService.ThemeChanged += handler;

            try
            {
                ThemeService.SetTheme(AppTheme.Light);
                Assert.Equal(AppTheme.Light, ThemeService.CurrentTheme);
                Assert.True(ThemeService.IsLightTheme);
                Assert.False(ThemeService.IsDarkTheme);
                Assert.Equal(AppTheme.Light, receivedTheme);
            }
            finally
            {
                ThemeService.ThemeChanged -= handler;
                ThemeService.SetTheme(AppTheme.Dark);
            }
        }

        [Fact]
        public void ThemeService_ToggleTheme_SwitchesAccurately()
        {
            ThemeService.SetTheme(AppTheme.Dark);
            Assert.Equal(AppTheme.Dark, ThemeService.CurrentTheme);

            ThemeService.ToggleTheme();
            Assert.Equal(AppTheme.Light, ThemeService.CurrentTheme);

            ThemeService.ToggleTheme();
            Assert.Equal(AppTheme.Dark, ThemeService.CurrentTheme);
        }

        [Fact]
        public void ThemeService_PersistsToDisk_AndRestoresOnInitialize()
        {
            try
            {
                ThemeService.SetTheme(AppTheme.Light);
                Assert.True(ThemeService.IsLightTheme);

                // Initialize again simulating app restart
                ThemeService.Initialize();
                Assert.Equal(AppTheme.Light, ThemeService.CurrentTheme);
            }
            finally
            {
                ThemeService.SetTheme(AppTheme.Dark);
            }
        }
    }
}
