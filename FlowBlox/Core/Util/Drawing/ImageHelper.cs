using FlowBlox.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FlowBlox.Core.Util.Drawing
{
    internal static class ImageHelper
    {
        public static Image CopyImage(Image source, int width, int height)
        {
            Bitmap target = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                int x = (width - source.Width) / 2;
                int y = (height - source.Height) / 2;
                
                g.DrawImage(source, x, y, source.Width, source.Height);
            }

            return target;
        }

        public static Icon ConvertImageToIcon(Image image, int size = 16)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            // In 32bpp konvertieren, falls nötig
            using (Bitmap bitmap = new Bitmap(image, new Size(size, size)))
            {
                IntPtr hIcon = bitmap.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }
    }
}
