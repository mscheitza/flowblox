using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlowBlox.UICore.Utilities
{
    public static class WpfIconHelper
    {
        public static ImageSource CreateMaterialIcon(
            PackIconMaterialKind kind,
            double size = 16,
            Brush foreground = null)
        {
            var icon = new PackIconMaterial
            {
                Kind = kind,
                Width = size,
                Height = size,
                Foreground = foreground ?? Brushes.Gray
            };

            icon.Measure(new Size(size, size));
            icon.Arrange(new Rect(0, 0, size, size));
            icon.UpdateLayout();

            var renderBitmap = new RenderTargetBitmap(
                (int)size,
                (int)size,
                96,
                96,
                PixelFormats.Pbgra32);

            renderBitmap.Render(icon);
            renderBitmap.Freeze();

            return renderBitmap;
        }
    }
}
