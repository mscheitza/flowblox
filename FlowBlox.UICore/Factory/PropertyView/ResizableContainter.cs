using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowBlox.UICore.Factory.PropertyView
{
    public static class ResizableControlContainer
    {
        public static System.Windows.Controls.Grid Create(FrameworkElement innerControl, int? minHeight = null)
        {
            var container = new System.Windows.Controls.Grid
            {
                Background = Brushes.Transparent
            };

            // ResizeGrip
            var resizeGrip = new ResizeGrip
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNWSE
            };

            // Add to Grid
            container.Children.Add(innerControl);
            container.Children.Add(resizeGrip);

            // Set min height
            if (minHeight.HasValue)
                container.MinHeight = minHeight.Value;

            // Mouse resizing logic
            resizeGrip.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    resizeGrip.CaptureMouse();
            };

            resizeGrip.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed && resizeGrip.IsMouseCaptured)
                {
                    var position = e.GetPosition(container);

                    if (position.Y > innerControl.MinHeight)
                        innerControl.Height = position.Y;
                }
            };

            resizeGrip.MouseUp += (s, e) =>
            {
                if (resizeGrip.IsMouseCaptured)
                    resizeGrip.ReleaseMouseCapture();
            };

            return container;
        }
    }
}
