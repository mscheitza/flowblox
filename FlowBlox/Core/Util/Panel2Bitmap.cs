using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace FlowBlox.Core.Util
{
    internal class Panel2Bitmap
    {
        public class ComposedImage
        {
            public Size dimensions;
            public List<ImagePart> images;

            public ComposedImage(Size dimensions)
            {
                this.dimensions = dimensions;
                this.images = new List<ImagePart>();
            }

            public ComposedImage(Size dimensions, List<ImagePart> images)
            {
                this.dimensions = dimensions;
                this.images = images;
            }

            public Bitmap composeImage()
            {
                if (dimensions == null || images == null)
                    return null;

                Bitmap fullbmp = new Bitmap(dimensions.Width, dimensions.Height);
                using (Graphics grD = Graphics.FromImage(fullbmp))
                {
                    foreach (ImagePart bmp in images)
                    {
                        grD.DrawImage(bmp.image, bmp.location.X, bmp.location.Y);
                    }
                }
                return fullbmp;
            }
        }

        public class ImagePart
        {
            public Point location;
            public Bitmap image;

            public ImagePart(Point location, Bitmap image)
            {
                this.location = location;
                this.image = image;
            }
        }

        public static void SaveBitmap(Panel CtrlToSave, string fileName)
        {
            Point oldPosition = new Point(CtrlToSave.HorizontalScroll.Value, CtrlToSave.VerticalScroll.Value);

            CtrlToSave.PerformLayout();

            ComposedImage ci = new ComposedImage(new Size(CtrlToSave.DisplayRectangle.Width, CtrlToSave.DisplayRectangle.Height));

            int visibleWidth = CtrlToSave.Width - (CtrlToSave.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0);
            int visibleHeightBuffer = CtrlToSave.Height - (CtrlToSave.HorizontalScroll.Visible ? SystemInformation.HorizontalScrollBarHeight : 0);

            //int Iteration = 0;

            for (int x = CtrlToSave.DisplayRectangle.Width - visibleWidth; x >= 0; x -= visibleWidth)
            {

                int visibleHeight = visibleHeightBuffer;

                for (int y = CtrlToSave.DisplayRectangle.Height - visibleHeight; y >= 0; y -= visibleHeight)
                {
                    CtrlToSave.HorizontalScroll.Value = x;
                    CtrlToSave.VerticalScroll.Value = y;

                    CtrlToSave.PerformLayout();

                    Bitmap bmp = new Bitmap(visibleWidth, visibleHeight);

                    CtrlToSave.DrawToBitmap(bmp, new Rectangle(0, 0, visibleWidth, visibleHeight));
                    ci.images.Add(new ImagePart(new Point(x, y), bmp));

                    if (y - visibleHeight < (CtrlToSave.DisplayRectangle.Height % visibleHeight))
                        visibleHeight = CtrlToSave.DisplayRectangle.Height % visibleHeight;

                    if (visibleHeight == 0)
                        break;
                }

                if (x - visibleWidth < (CtrlToSave.DisplayRectangle.Width % visibleWidth))
                    visibleWidth = CtrlToSave.DisplayRectangle.Width % visibleWidth;
                if (visibleWidth == 0)
                    break;
            }

            Bitmap img = ci.composeImage();
            System.IO.FileStream fStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            img.Save(fStream, System.Drawing.Imaging.ImageFormat.Bmp);
            fStream.Close();

            CtrlToSave.HorizontalScroll.Value = oldPosition.X;
            CtrlToSave.VerticalScroll.Value = oldPosition.Y;
        }
    }
}
