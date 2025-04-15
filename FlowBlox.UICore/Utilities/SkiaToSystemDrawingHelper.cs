using System;
using System.Drawing;
using System.IO;
using SkiaSharp;

namespace FlowBlox.UICore.Utilities
{
    /// <summary>
    /// Utility class for converting between SkiaSharp images and System.Drawing.Image.
    /// Intended for Windows-specific UI code (WinForms/WPF).
    /// </summary>
    public static class SkiaToSystemDrawingHelper
    {
        /// <summary>
        /// Converts a SkiaSharp SKImage to a System.Drawing.Image.
        /// The image is encoded as PNG in-memory to ensure compatibility.
        /// </summary>
        /// <param name="skImage">The SKImage to convert.</param>
        public static Image ToSystemDrawingImage(SKImage skImage)
        {
            if (skImage == null)
                throw new ArgumentNullException(nameof(skImage));

            using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream(data.ToArray());
            return Image.FromStream(ms);
        }

        /// <summary>
        /// Converts a SkiaSharp SKBitmap to a System.Drawing.Image.
        /// </summary>
        /// <param name="skBitmap">The SKBitmap to convert.</param>
        public static Image ToSystemDrawingImage(SKBitmap skBitmap)
        {
            if (skBitmap == null)
                throw new ArgumentNullException(nameof(skBitmap));

            using var image = SKImage.FromBitmap(skBitmap);
            return ToSystemDrawingImage(image);
        }

        /// <summary>
        /// Converts a System.Drawing.Image to a SkiaSharp SKImage.
        /// Useful for processing legacy WinForms/WPF images in SkiaSharp.
        /// </summary>
        /// <param name="image">The System.Drawing.Image to convert.</param>
        public static SKImage ToSkiaImage(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            return SKImage.FromEncodedData(ms);
        }
    }
}
