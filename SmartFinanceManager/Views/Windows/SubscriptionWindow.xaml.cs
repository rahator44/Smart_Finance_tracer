using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;
using SmartFinanceManager.ViewModels;

namespace SmartFinanceManager.Views.Windows
{
    public partial class SubscriptionWindow : Window
    {
        public SubscriptionViewModel ViewModel { get; }

        public SubscriptionWindow(SubscriptionService? subscriptionService = null, User? user = null)
        {
            InitializeComponent();
            ViewModel = new SubscriptionViewModel(subscriptionService, user);
            DataContext = ViewModel;

            ViewModel.RequestClose += () => Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FlipToBack_Click(object sender, RoutedEventArgs e)
        {
            if (Resources["FlipToBackAnimation"] is Storyboard flipToBack)
            {
                flipToBack.Begin();
            }
            else
            {
                FrontCard.Visibility = Visibility.Collapsed;
                BackCard.Visibility = Visibility.Visible;
            }
            ViewModel.IsFlipped = true;
        }

        private void FlipToFront_Click(object sender, RoutedEventArgs e)
        {
            if (Resources["FlipToFrontAnimation"] is Storyboard flipToFront)
            {
                flipToFront.Begin();
            }
            else
            {
                BackCard.Visibility = Visibility.Collapsed;
                FrontCard.Visibility = Visibility.Visible;
            }
            ViewModel.IsFlipped = false;
        }
    }
}
