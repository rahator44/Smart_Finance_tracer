using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDarkTheme => ThemeService.IsDarkTheme;
        public bool IsLightTheme => ThemeService.IsLightTheme;
        public string CurrentThemeLabel => ThemeService.IsDarkTheme ? "Light Mode" : "Dark Mode";
        public string CurrentThemeIcon => ThemeService.IsDarkTheme ? "IconSun" : "IconMoon";

        public ICommand ToggleThemeCommand { get; }

        protected ViewModelBase()
        {
            ToggleThemeCommand = new RelayCommand(ExecuteToggleTheme);
            ThemeService.ThemeChanged += OnThemeServiceThemeChanged;
        }

        private void OnThemeServiceThemeChanged(AppTheme theme)
        {
            OnPropertyChanged(nameof(IsDarkTheme));
            OnPropertyChanged(nameof(IsLightTheme));
            OnPropertyChanged(nameof(CurrentThemeLabel));
            OnPropertyChanged(nameof(CurrentThemeIcon));
        }

        protected virtual void ExecuteToggleTheme()
        {
            ThemeService.ToggleTheme();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
