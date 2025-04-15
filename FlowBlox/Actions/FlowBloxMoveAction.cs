using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.UICore.Actions;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlowBlox.Actions
{
    public class FlowBloxMoveAction : FlowBloxBaseAction
    {
        public FlowBloxMoveAction() : base()
        {
            
        }

        public FlowBlockUIElement UIElement { get; set; }

        public Point CapturedAutoScrollPosition { get; set; }

        public Point From { get; set; }

        public Point To { get; set; }

        private Point GetAutoScrollPositionDelta()
        {
            var currentAutoScrollPosition = ((Panel)UIElement.Parent).AutoScrollPosition;

            var delta = new Point()
            {
                X = currentAutoScrollPosition.X - CapturedAutoScrollPosition.X,
                Y = currentAutoScrollPosition.Y - CapturedAutoScrollPosition.Y
            };

            return delta;
        }

        public override void Undo()
        {
            var delta = GetAutoScrollPositionDelta();
            UIElement.Location = new Point()
            {
                X = delta.X + From.X,
                Y = delta.Y + From.Y
            };
            base.Undo();
        }

        public override void Invoke()
        {
            var delta = GetAutoScrollPositionDelta();
            UIElement.Location = new Point()
            {
                X = delta.X + To.X,
                Y = delta.Y + To.Y
            };
            base.Invoke();
        }
    }
}
