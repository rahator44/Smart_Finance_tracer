using System;
using System.Windows;
using SmartFinanceManager.Services;
using SmartFinanceManager.Views;
using SmartFinanceManager.Views.Windows;

namespace SmartFinanceManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize theme before showing any windows
            ThemeService.Initialize();

            // Initialize SQLite DB and seed test datasets
            DatabaseService.Initialize();

            // Check if user has a remembered active session
            if (AuthService.TryRestoreSession())
            {
                var user = AuthService.CurrentUser;
                var subService = new SubscriptionService();
                if (user != null && string.Equals(user.AccountType, "Office", StringComparison.OrdinalIgnoreCase) && !subService.HasOfficeAccess(user))
                {
                    // Office user without approved subscription: do not auto-login to personal account
                    var authService = new AuthService();
                    authService.Logout();

                    var loginWin = new LoginWindow();
                    loginWin.Show();

                    var subWin = new SubscriptionWindow(subService, user);
                    subWin.Owner = loginWin;
                    subWin.ShowDialog();
                }
                else
                {
                    var mainWin = new MainWindow();
                    mainWin.Show();
                }
            }
            else
            {
                var loginWin = new LoginWindow();
                loginWin.Show();
            }
        }
    }
}
