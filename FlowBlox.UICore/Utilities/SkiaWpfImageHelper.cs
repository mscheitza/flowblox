using SkiaSharp;
using System.IO;
using System.Windows.Media.Imaging;

namespace FlowBlox.UICore.Utilities
{
    public static class SkiaWpfImageHelper
    {
        public static BitmapImage ConvertToImageSource(SKImage image)
        {
            if (image == null)
                return new BitmapImage();

            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream(data.ToArray());

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}
