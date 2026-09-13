using System.Drawing;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlowBlox.UICore.PopUp.Utilities
{
    internal static class PopUpImageResourceHelper
    {
        public static ImageSource GetImageSource(ResourceManager resourceManager, string resourceName)
        {
            var resource = resourceManager.GetObject(resourceName);
            if (resource is not Bitmap bitmap)
                return null;

            var handle = bitmap.GetHbitmap();
            try
            {
                var source = Imaging.CreateBitmapSourceFromHBitmap(
                    handle,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                source.Freeze();
                return source;
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
