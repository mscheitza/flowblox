using System.Windows;
using System.Windows.Media;

namespace FlowBlox.UICore.Utilities
{
    public static class ArrowGeometryHelper
    {
        public static Point GetEdgePoint(Rect bounds, Point center, Point other)
        {
            var dx = other.X - center.X;
            var dy = other.Y - center.Y;
            if (Math.Abs(dx) < 0.001d && Math.Abs(dy) < 0.001d)
                return center;

            var tx = dx > 0d
                ? (bounds.Right - center.X) / dx
                : (bounds.Left - center.X) / dx;
            var ty = dy > 0d
                ? (bounds.Bottom - center.Y) / dy
                : (bounds.Top - center.Y) / dy;

            var t = Math.Min(Math.Abs(tx), Math.Abs(ty));
            return new Point(center.X + dx * t, center.Y + dy * t);
        }

        public static PointCollection CreateArrowHeadPoints(Point start, Point end, double length = 11d, double width = 5d)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var lineLength = Math.Sqrt(dx * dx + dy * dy);
            if (lineLength < 0.001d)
                return new PointCollection { end, end, end };

            var ux = dx / lineLength;
            var uy = dy / lineLength;
            var basePoint = new Point(end.X - ux * length, end.Y - uy * length);
            var perpendicular = new Vector(-uy, ux);

            return new PointCollection
            {
                end,
                new Point(basePoint.X + perpendicular.X * width, basePoint.Y + perpendicular.Y * width),
                new Point(basePoint.X - perpendicular.X * width, basePoint.Y - perpendicular.Y * width)
            };
        }

        public static Vector GetPerpendicularOffset(Point start, Point end, double offset)
        {
            if (Math.Abs(offset) < 0.001d)
                return new Vector();

            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.001d)
                return new Vector();

            return new Vector(-dy / length * offset, dx / length * offset);
        }
    }
}
