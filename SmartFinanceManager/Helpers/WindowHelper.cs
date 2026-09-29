using System.Windows;

namespace SmartFinanceManager.Helpers
{
    public static class WindowHelper
    {
        /// <summary>
        /// Transfers window state (Maximized / Normal), position, and dimensions between windows
        /// so that window transitions (such as Logout or Login) remain completely seamless.
        /// </summary>
        public static void TransferWindowState(Window from, Window to)
        {
            if (from == null || to == null) return;

            to.WindowStartupLocation = WindowStartupLocation.Manual;

            if (from.WindowState == WindowState.Maximized)
            {
                // Preserve previous restore bounds so restoring the window later returns to appropriate bounds
                if (!from.RestoreBounds.IsEmpty && from.RestoreBounds.Width > 0 && from.RestoreBounds.Height > 0)
                {
                    to.Left = from.RestoreBounds.Left;
                    to.Top = from.RestoreBounds.Top;
                    to.Width = from.RestoreBounds.Width;
                    to.Height = from.RestoreBounds.Height;
                }
                else
                {
                    to.Left = from.Left;
                    to.Top = from.Top;
                    to.Width = from.Width;
                    to.Height = from.Height;
                }

                to.WindowState = WindowState.Maximized;
            }
            else
            {
                to.WindowState = WindowState.Normal;
                to.Left = from.Left;
                to.Top = from.Top;

                double sourceWidth = from.ActualWidth > 0 ? from.ActualWidth : from.Width;
                double sourceHeight = from.ActualHeight > 0 ? from.ActualHeight : from.Height;

                if (sourceWidth >= to.MinWidth)
                {
                    to.Width = sourceWidth;
                }
                if (sourceHeight >= to.MinHeight)
                {
                    to.Height = sourceHeight;
                }
            }
        }
    }
}
