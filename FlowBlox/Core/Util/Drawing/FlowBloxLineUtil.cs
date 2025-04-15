using FlowBlox.Core.Models.Drawing;
using FlowBlox.Grid.Elements.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Util.Drawing
{
    internal class FlowBloxLineUtil
    {
        public static void PrintLine(Graphics graphics, IEnumerable<FlowBlockUIElement> uiElements, FlowBloxArrow flowBloxArrow, bool dashed = false, string text = "", Color? lineColor = null)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));

            if (flowBloxArrow == null)
                throw new ArgumentNullException(nameof(flowBloxArrow));

            int centerX = (int)flowBloxArrow.StartPoint.X;
            int centerY = (int)flowBloxArrow.StartPoint.Y;
            int nextCenterX = (int)flowBloxArrow.EndPoint.X;
            int nextCenterY = (int)flowBloxArrow.EndPoint.Y;

            // Schnittpunkt ermitteln und Pfeilkopf zeichnen
            PointF intersection = GetClosestIntersectionPoint(centerX, centerY, nextCenterX, nextCenterY, flowBloxArrow.To);

            // Berechne die Länge des Pfeilkopfes
            const float arrowSize = 10f;

            // Berechne den Punkt, an dem die Linie enden soll, so dass der Pfeilkopf dort beginnt
            PointF lineEndPoint = GetLineEndPoint(centerX, centerY, intersection.X, intersection.Y, arrowSize);

            // Linie zeichnen
            Pen pen = new Pen(lineColor ?? Color.LavenderBlush, 2);
            if (dashed)
                pen.DashPattern = new float[] { 5, 5, 5, 5 };
            graphics.DrawLine(pen, new Point(centerX, centerY), lineEndPoint);

            // Pfeilkopf zeichnen
            DrawArrowhead(graphics, pen.Color, centerX, centerY, intersection.X, intersection.Y);

            if (!string.IsNullOrEmpty(text))
            {
                using Font font = new Font("Segoe UI", 9);
                SizeF textSize = graphics.MeasureString(text, font);
                PointF? bestTextPosition = FindBestTextPosition(graphics, uiElements, text, font, centerX, centerY, nextCenterX, nextCenterY, textSize);
                if (bestTextPosition.HasValue)
                {
                    SolidBrush textBrush = new SolidBrush(Color.LightGoldenrodYellow);
                    graphics.DrawString(text, font, textBrush, bestTextPosition.Value);
                }
            }
        }

        private static PointF GetLineEndPoint(float startX, float startY, float endX, float endY, float offset)
        {
            double angle = Math.Atan2(endY - startY, endX - startX);
            return new PointF(
                endX - offset * (float)Math.Cos(angle),
                endY - offset * (float)Math.Sin(angle)
            );
        }

        private static PointF GetClosestIntersectionPoint(int startX, int startY, int endX, int endY, FlowBlockUIElement to)
        {
            Rectangle toRect = to.Bounds;
            PointF[] intersectionPoints = new PointF[4];
            bool[] validIntersections = new bool[4];

            // Calculate potential intersection points with each side of the rectangle
            validIntersections[0] = GetIntersectionWithLine(startX, startY, endX, endY, toRect.Left, toRect.Top, toRect.Right, toRect.Top, out intersectionPoints[0]);
            validIntersections[1] = GetIntersectionWithLine(startX, startY, endX, endY, toRect.Right, toRect.Top, toRect.Right, toRect.Bottom, out intersectionPoints[1]);
            validIntersections[2] = GetIntersectionWithLine(startX, startY, endX, endY, toRect.Right, toRect.Bottom, toRect.Left, toRect.Bottom, out intersectionPoints[2]);
            validIntersections[3] = GetIntersectionWithLine(startX, startY, endX, endY, toRect.Left, toRect.Bottom, toRect.Left, toRect.Top, out intersectionPoints[3]);

            // Find the closest valid intersection
            PointF closestPoint = new PointF(endX, endY);
            double closestDistance = double.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                if (validIntersections[i])
                {
                    double distance = Math.Sqrt((intersectionPoints[i].X - startX) * (intersectionPoints[i].X - startX) + (intersectionPoints[i].Y - startY) * (intersectionPoints[i].Y - startY));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPoint = intersectionPoints[i];
                    }
                }
            }

            return closestPoint;
        }

        private static bool GetIntersectionWithLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, out PointF intersection)
        {
            intersection = new PointF();

            float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (denom == 0) return false;

            float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
            float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                intersection.X = x1 + ua * (x2 - x1);
                intersection.Y = y1 + ua * (y2 - y1);
                return true;
            }

            return false;
        }

        private static void DrawArrowhead(Graphics graphics, Color color, int startX, int startY, float endX, float endY)
        {
            const int arrowSize = 10;
            double angle = Math.Atan2(endY - startY, endX - startX);

            PointF[] arrowPoints = new PointF[3];
            arrowPoints[0] = new PointF(endX, endY);
            arrowPoints[1] = new PointF(
                endX - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                endY - arrowSize * (float)Math.Sin(angle - Math.PI / 6));
            arrowPoints[2] = new PointF(
                endX - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                endY - arrowSize * (float)Math.Sin(angle + Math.PI / 6));

            using (Brush brush = new SolidBrush(color))
            {
                graphics.FillPolygon(brush, arrowPoints);
            }
        }

        private static PointF? FindBestTextPosition(Graphics graphics, IEnumerable<FlowBlockUIElement> uiElements, string text, Font font, int startX, int startY, int endX, int endY, SizeF textSize)
        {
            const float initialPosition = 0.25f;
            const float step = 0.10f;
            const float maxPosition = 0.90f;
            float currentPos = initialPosition;

            // Initial text position
            float textX = startX + currentPos * (endX - startX) - textSize.Width / 2;
            float textY = startY + currentPos * (endY - startY) - textSize.Height / 2 - 10;
            RectangleF textRect = new RectangleF(textX, textY, textSize.Width, textSize.Height);

            // Check for boundary intersections and adjust position
            while (uiElements.Any(e => e.Bounds.IntersectsWith(Rectangle.Round(textRect))) && currentPos < maxPosition)
            {
                currentPos += step;
                textX = startX + currentPos * (endX - startX) - textSize.Width / 2;
                textY = startY + currentPos * (endY - startY) - textSize.Height / 2 - 10;
                textRect = new RectangleF(textX, textY, textSize.Width, textSize.Height);
            }

            // If we exceed the maximum position and no suitable position is found, return null
            if (currentPos >= maxPosition && uiElements.Any(e => e.Bounds.IntersectsWith(Rectangle.Round(textRect))))
                return null;

            return new PointF(textRect.X, textRect.Y);
        }
    }
}
