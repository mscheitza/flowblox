using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowBlox.AppWindow
{
    internal sealed class WaitOverlayWindow : MetroWindow
    {
        public WaitOverlayWindow()
        {
            Width = 140;
            Height = 86;
            ShowInTaskbar = false;
            ShowTitleBar = false;
            ShowCloseButton = false;
            ShowMinButton = false;
            ShowMaxRestoreButton = false;
            ShowIconOnTitleBar = false;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            BorderBrush = Brushes.Transparent;
            GlowBrush = Brushes.Transparent;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Topmost = false;
            ShowActivated = false;
            IsHitTestVisible = false;

            var container = new Border
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(210, 68, 68, 68)),
                Padding = new Thickness(10)
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var ring = new ProgressRing
            {
                IsActive = true,
                Width = 32,
                Height = 32,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var label = new TextBlock
            {
                Text = "Loading project...",
                Foreground = Brushes.WhiteSmoke,
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stack.Children.Add(ring);
            stack.Children.Add(label);
            container.Child = stack;
            Content = container;
        }
    }
}
