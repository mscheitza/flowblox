using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// Reproducible animation from the actual logo. No external encoder or runtime GIF package required.
// dotnet run --project scripts/FlowBlox.SplashAssets -- <repository-root>
internal static class Program
{
    private const int Width = 820, Height = 180;
    private const double PlaybackSpeed = 2.0;
    private const double TransformStart = .55, TransformEnd = 2.1;
    private static readonly Color Cyan = Color.FromRgb(105, 196, 228);

    [STAThread]
    private static void Main(string[] args)
    {
        var root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
        var source = new BitmapImage(new Uri(Path.Combine(root, "FlowBlox Images/FlowBloxMainLogo_1240_600.png")));
        // Include padding below the baseline and around the outlines, including antialiasing.
        var prefix = Prefix(source);
        var finalX = new CroppedBitmap(source, new Int32Rect(960, 188, 145, 205));
        var c = Glyph("c", 78, 83, 565, 63);
        var k = Glyph("k", 80, 117, 648, 29);
        var s = Glyph("s", 75, 83, 733, 63);
        var x = ImagePoints(finalX, 553.5, 22.5, 94.25, 133.25);
        x = Sort(x, 597, 88);
        var letters = new[] { Sort(c, 604, 104), Sort(k, 688, 88), Sort(s, 770, 104) };
        var from = new List<Point>();
        var to = new List<Point>();
        foreach (var letter in letters)
        {
            // Each letter contributes to the entire x, so all three reconstruct it in parallel.
            for (int i = 0; i < letter.Length; i++)
            {
                from.Add(letter[i]);
                to.Add(x[i * x.Length / letter.Length]);
            }
        }
        double leftEdge = ImagePoints(prefix, 33.5, 22.5, 533, 133.25).Min(p => p.X);
        var gif = new GifBitmapEncoder();
        var palette = Palette();
        for (int frame = 0; frame < 80; frame++)
        {
            double time = frame * .05;
            double progress = Smooth((time - TransformStart) / (TransformEnd - TransformStart));
            double heat = HeatAt(time);
            var positions = new Point[from.Count];
            for (int i = 0; i < positions.Length; i++)
            {
                var a = from[i]; var b = to[i];
                // Short contour segments follow restrained curved paths into the new glyph.
                double arc = Math.Sin(progress * Math.PI) * Math.Sin(i / 5 * 2.399) * 3;
                positions[i] = new Point(a.X + (b.X - a.X) * progress,
                    a.Y + (b.Y - a.Y) * progress + arc);
            }
            double rightEdge = positions.Max(p => p.X) + 1.15;
            double offset = Width / 2.0 - (leftEdge + rightEdge) / 2;
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(53, 53, 53)), null, new Rect(0, 0, Width, Height));
                // Center the current geometry on every frame, including the contracting suffix.
                dc.PushTransform(new TranslateTransform(offset, 0));
                dc.DrawImage(prefix, new Rect(33.5, 22.5, 533, 133.25));
                // Finish the motion before blending the aligned contour into the original x.
                // This prevents an early bitmap handover while segments are still travelling.
                double materialized = Smooth((time - TransformEnd) / .4);
                if (materialized < 1)
                {
                    dc.PushOpacity(1 - materialized);
                    DrawReconstruction(dc, positions, from, to, heat, progress);
                    dc.Pop();
                }
                if (materialized > 0)
                {
                    dc.PushOpacity(materialized);
                    dc.DrawImage(finalX, new Rect(553.5, 22.5, 94.25, 133.25));
                    Glow(dc, x, heat);
                    dc.PushOpacity(heat * .45);
                    DrawReconstruction(dc, x, null, null, 0, 0);
                    dc.Pop();
                    dc.Pop();
                }
                dc.Pop();
            }
            var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var metadata = new BitmapMetadata("gif");
            metadata.SetQuery("/grctlext/Delay", (ushort)5);
            metadata.SetQuery("/grctlext/Disposal", (byte)1);
            var indexed = new FormatConvertedBitmap(bitmap, PixelFormats.Indexed8, palette, 0);
            gif.Frames.Add(BitmapFrame.Create(indexed, null, metadata, null));
            if (frame is 0 or 16 or 24 or 32 or 40 or 48 or 56 or 64 or 79)
            {
                var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                var previewDir = Path.Combine(root, "artifacts/splash-preview");
                Directory.CreateDirectory(previewDir);
                using var preview = File.Create(Path.Combine(previewDir, $"frame-{frame:00}.png"));
                png.Save(preview);
            }
        }
        var output = Path.Combine(root, "FlowBlox.UICore/Resources/Images/FlowBloxStartup.gif");
        using var encoded = new MemoryStream(); gif.Save(encoded);
        var bytes = encoded.ToArray();
        bytes = SetFrameDelays(bytes, 5 / PlaybackSpeed);
        var decoded = new GifBitmapDecoder(new MemoryStream(bytes), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        int duration = decoded.Frames.Sum(f => Convert.ToInt32(((BitmapMetadata)f.Metadata).GetQuery("/grctlext/Delay")) * 10);
        if (decoded.Frames.Count != 80 || duration != (int)(4000 / PlaybackSpeed))
            throw new InvalidDataException($"Invalid GIF timing: {decoded.Frames.Count} frames, {duration} ms.");
        File.WriteAllBytes(output, bytes);
        Console.WriteLine($"Saved {output}: {gif.Frames.Count} frames, {4 / PlaybackSpeed} seconds ({PlaybackSpeed}x speed), one playback, final logo held.");
    }

    private static BitmapPalette Palette()
    {
        var colors = new List<Color> { Color.FromRgb(53, 53, 53) };
        for (int i = 0; i < 32; i++) colors.Add(Color.FromRgb((byte)(30 + i * 2), (byte)(30 + i * 2), (byte)(30 + i * 2)));
        for (int i = 0; i < 160; i++)
        {
            double t = i / 159.0;
            colors.Add(Color.FromRgb((byte)(53 + 52 * t), (byte)(53 + 143 * t), (byte)(53 + 175 * t)));
        }
        for (int i = 0; i < 63; i++)
        {
            double t = i / 62.0;
            colors.Add(Color.FromRgb((byte)(105 + 115 * t), (byte)(196 + 50 * t), (byte)(228 + 27 * t)));
        }
        return new BitmapPalette(colors);
    }

    private static byte[] SetFrameDelays(byte[] bytes, double delay)
    {
        // WIC's GIF encoder drops the per-frame delay supplied through BitmapMetadata.
        // Rebuild graphic control blocks, including when WIC omitted them entirely.
        int index = 13;
        int frame = 0;
        if ((bytes[10] & 128) != 0) index += 3 * (1 << ((bytes[10] & 7) + 1));
        using var result = new MemoryStream();
        result.Write(bytes, 0, index);
        while (index < bytes.Length)
        {
            int start = index;
            bool skip = false;
            byte marker = bytes[index++];
            if (marker == 0x3b) { result.WriteByte(marker); break; }
            if (marker == 0x21)
            {
                byte label = bytes[index++];
                skip = label == 0xf9;
            }
            else if (marker == 0x2c)
            {
                // Whole centiseconds: alternate 2/3 cs for 2x speed, preserving all frames.
                int frameDelay = (int)Math.Round((frame + 1) * delay) - (int)Math.Round(frame * delay);
                result.Write(new byte[] { 0x21, 0xf9, 4, 4, (byte)(frameDelay & 255), (byte)(frameDelay >> 8), 0, 0 });
                frame++;
                byte flags = bytes[index + 8]; index += 9;
                if ((flags & 128) != 0) index += 3 * (1 << ((flags & 7) + 1));
                index++; // LZW minimum code size.
            }
            else throw new InvalidDataException($"Unexpected GIF block: {marker:X2}");
            int length;
            while ((length = bytes[index++]) != 0) index += length;
            if (!skip) result.Write(bytes, start, index - start);
        }
        return result.ToArray();
    }

    private static BitmapSource Prefix(BitmapSource source)
    {
        // The bounding boxes of the final o and x overlap. Isolate the round o contour
        // instead of slicing through its right edge or retaining fragments of the x.
        var crop = new CroppedBitmap(source, new Int32Rect(160, 188, 820, 205));
        var bitmap = new FormatConvertedBitmap(crop, PixelFormats.Bgra32, null, 0);
        var pixels = new byte[820 * 205 * 4];
        bitmap.CopyPixels(pixels, 820 * 4, 0);
        for (int y = 0; y < 205; y++)
            for (int px = 795; px < 820; px++)
            {
                double dx = (px + 160 - 900) / 70.0, dy = (y + 188 - 314) / 70.0;
                if (dx * dx + dy * dy <= 1) continue;
                int i = (y * 820 + px) * 4;
                pixels[i] = pixels[i + 1] = pixels[i + 2] = 53; pixels[i + 3] = 255;
            }
        return BitmapSource.Create(820, 205, 96, 96, PixelFormats.Bgra32, null, pixels, 820 * 4);
    }

    private static Point[] Glyph(string text, double width, double height, double left, double top)
    {
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface("Century Gothic"), 160, Brushes.White, 1);
        var geometry = formatted.BuildGeometry(new Point());
        var bounds = geometry.Bounds;
        geometry.Transform = new MatrixTransform(width / bounds.Width, 0, 0, height / bounds.Height,
            left - bounds.X * width / bounds.Width, top - bounds.Y * height / bounds.Height);
        var outline = geometry.GetWidenedPathGeometry(new Pen(Brushes.White, 1.8));
        var points = new List<Point>();
        for (double y = outline.Bounds.Top - 1; y <= outline.Bounds.Bottom + 1; y += .8)
            for (double px = outline.Bounds.Left - 1; px <= outline.Bounds.Right + 1; px += .8)
                if (outline.FillContains(new Point(px, y))) points.Add(new Point(px, y));
        return points.ToArray();
    }

    private static Point[] ImagePoints(BitmapSource image, double left, double top, double width, double height)
    {
        var bitmap = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
        var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
        var points = new List<Point>();
        for (int y = 0; y < bitmap.PixelHeight; y++)
            for (int px = 0; px < bitmap.PixelWidth; px++)
            {
                int i = (y * bitmap.PixelWidth + px) * 4;
                if (pixels[i] > 110 && pixels[i + 1] > 100)
                    points.Add(new Point(left + px * width / bitmap.PixelWidth, top + y * height / bitmap.PixelHeight));
            }
        return points.ToArray();
    }

    private static Point[] Sort(Point[] points, double cx, double cy) =>
        points.OrderBy(p => Math.Atan2(p.Y - cy, p.X - cx)).ThenBy(p => (p.X - cx) * (p.X - cx) + (p.Y - cy) * (p.Y - cy)).ToArray();

    private static double Smooth(double progress)
    {
        double t = Math.Clamp(progress, 0, 1);
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static double HeatAt(double time)
    {
        if (time <= TransformStart) return .5 * Smooth((time - .25) / .3);
        if (time <= TransformEnd)
            return .5 * (1 - Smooth((time - TransformStart) / (TransformEnd - TransformStart)));
        // Begin the ice-blue pulse as soon as the segments reach x, overlapping the
        // final contour blend so the movement and highlight form one continuous finish.
        // A single sine-squared pulse has a rounded peak rather than the flat apex
        // produced by joining two quintic easing curves. Glow thickness follows it too.
        double phase = Math.Clamp((time - TransformEnd) / (3.55 - TransformEnd), 0, 1);
        double pulse = Math.Sin(Math.PI * phase);
        return .7 * pulse * pulse;
    }

    private static SolidColorBrush LitBrush(double heat) => new(Color.FromRgb(
        (byte)(Cyan.R + (210 - Cyan.R) * heat),
        (byte)(Cyan.G + (242 - Cyan.G) * heat),
        (byte)(Cyan.B + (255 - Cyan.B) * heat)));

    private static void Glow(DrawingContext dc, Point[] points, double heat)
    {
        if (heat <= 0) return;
        var halo = new SolidColorBrush(Color.FromArgb((byte)(heat * 20), 130, 220, 250));
        foreach (var point in points) dc.DrawEllipse(halo, null, point, 2.8, 2.8);
    }

    private static void DrawReconstruction(DrawingContext dc, Point[] positions, List<Point>? from,
        List<Point>? to, double heat, double progress)
    {
        Glow(dc, positions, heat);
        var brush = LitBrush(heat);
        var pen = new Pen(brush, .9);
        // Avoid a bright solid mass where the three source outlines converge.
        dc.PushOpacity(1 - progress * .6);
        for (int i = 0; i < positions.Length; i++)
        {
            dc.DrawRectangle(brush, null, new Rect(positions[i], new Size(1.15, 1.15)));
            if (from != null && to != null && i > 0 && i % 5 != 0 && (from[i] - from[i - 1]).Length < 2.5 &&
                (to[i] - to[i - 1]).Length < 2.5)
                dc.DrawLine(pen, positions[i - 1], positions[i]);
        }
        dc.Pop();
    }
}
