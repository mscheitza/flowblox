using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.Drawing;
using FlowBlox.Grid.Elements.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.Drawing
{
    public class FlowBloxArrow
    {
        public PointF StartPoint { get; }
        public PointF EndPoint { get; }
        public FlowBlockUIElement From { get; }
        public FlowBlockUIElement To { get; }

        public FlowBloxArrow(PointF startPoint, PointF endPoint, FlowBlockUIElement from, FlowBlockUIElement to)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            From = from;
            To = to;
        }

        public FlowBloxArrow(FlowBlockUIElement from, FlowBlockUIElement to, float offset = 0f)
            : this(
                new PointF(
                    from.Location.X + (from.Width / 2f) + offset,
                    from.Location.Y + (from.Height / 2f) + offset),
                new PointF(
                    to.Location.X + (to.Width / 2f) + offset,
                    to.Location.Y + (to.Height / 2f) + offset),
                from,
                to)
        {
        }

        public bool IntersectsWith(Point point)
        {
            const float tolerance = 5f;
            return DistanceToPoint(point) <= tolerance;
        }

        private float DistanceToPoint(Point point)
        {
            float dx = EndPoint.X - StartPoint.X;
            float dy = EndPoint.Y - StartPoint.Y;

            float lengthSquared = dx * dx + dy * dy;
            float projection = ((point.X - StartPoint.X) * dx + (point.Y - StartPoint.Y) * dy) / lengthSquared;

            if (projection < 0 || projection > 1)
                return float.MaxValue;

            float closestX = StartPoint.X + projection * dx;
            float closestY = StartPoint.Y + projection * dy;

            float distanceX = closestX - point.X;
            float distanceY = closestY - point.Y;

            return (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
        }

        public override bool Equals(object obj)
        {
            if (obj is FlowBloxArrow arrow)
            {
                return StartPoint == arrow.StartPoint && 
                       EndPoint == arrow.EndPoint &&
                       From == arrow.From && 
                       To == arrow.To;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartPoint, EndPoint, From, To);
        }
    }
}
