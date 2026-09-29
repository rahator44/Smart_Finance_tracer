using System.Windows;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;
using SmartFinanceManager.ViewModels;
using SmartFinanceManager.Views.Windows;

namespace SmartFinanceManager.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var viewModel = new LoginViewModel();
            DataContext = viewModel;

            viewModel.RequestClose += Close;
            viewModel.RequestOpenRegister += () =>
            {
                var regWin = new RegisterWindow();
                WindowHelper.TransferWindowState(this, regWin);
                regWin.Show();
            };
            viewModel.RequestOpenMain += () =>
            {
                var mainWin = new MainWindow();
                WindowHelper.TransferWindowState(this, mainWin);
                mainWin.Show();
            };
            viewModel.RequestOpenSubscription += (user) =>
            {
                var subService = new SubscriptionService();
                var subWin = new SubscriptionWindow(subService, user);
                subWin.Owner = this;
                subWin.ShowDialog();

                // Refresh status message after subscription window closes
                var status = subService.GetSubscriptionStatus(user.Id);
                if (status == SubscriptionStatus.Pending)
                {
                    viewModel.ErrorMessage = "Subscription submitted! Awaiting administrator approval.";
                }
                else if (status == SubscriptionStatus.Approved)
                {
                    viewModel.ErrorMessage = "Subscription Approved! You can now log in.";
                }
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    var mousePos = PointToScreen(e.GetPosition(this));
                    WindowState = WindowState.Normal;
                    Top = mousePos.Y - 20;
                    Left = mousePos.X - (Width / 2);
                }
                DragMove();
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                if (vm.IsPasswordVisible)
                {
                    PasswordInput.Password = VisiblePasswordInput.Text;
                    vm.IsPasswordVisible = false;
                }
                else
                {
                    VisiblePasswordInput.Text = PasswordInput.Password;
                    vm.IsPasswordVisible = true;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                if (vm.IsPasswordVisible)
                {
                    vm.Password = VisiblePasswordInput.Text;
                }
                else
                {
                    vm.Password = PasswordInput.Password;
                }
                vm.LoginCommand.Execute(null);
            }
        }
    }
}
