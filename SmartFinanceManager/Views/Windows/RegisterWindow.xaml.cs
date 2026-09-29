using System.Windows;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.ViewModels;
using SmartFinanceManager.Views.Windows;

namespace SmartFinanceManager.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            var viewModel = new RegisterViewModel();
            DataContext = viewModel;

            viewModel.RequestClose += Close;
            viewModel.RequestOpenLogin += () =>
            {
                var loginWin = new LoginWindow();
                WindowHelper.TransferWindowState(this, loginWin);
                loginWin.Show();
            };
            viewModel.RequestOpenSubscription += () =>
            {
                var subWin = new SubscriptionWindow();
                subWin.Owner = this;
                subWin.ShowDialog();
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
            Close();
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                if (vm.IsPasswordVisible)
                {
                    PasswordInput.Password = VisiblePasswordInput.Text;
                    ConfirmPasswordInput.Password = VisibleConfirmPasswordInput.Text;
                    vm.IsPasswordVisible = false;
                }
                else
                {
                    VisiblePasswordInput.Text = PasswordInput.Password;
                    VisibleConfirmPasswordInput.Text = ConfirmPasswordInput.Password;
                    vm.IsPasswordVisible = true;
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                if (vm.IsPasswordVisible)
                {
                    vm.Password = VisiblePasswordInput.Text;
                    vm.ConfirmPassword = VisibleConfirmPasswordInput.Text;
                }
                else
                {
                    vm.Password = PasswordInput.Password;
                    vm.ConfirmPassword = ConfirmPasswordInput.Password;
                }
                vm.RegisterCommand.Execute(null);
            }
        }
    }
}
