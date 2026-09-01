using System.Diagnostics;

namespace FlowBlox.UICore.ViewModels.ProjectPanel
{
    internal sealed class FlowBlockNodeCenterPreservationGuard : IDisposable
    {
        private readonly FlowBlockNodeViewModel _node;
        private bool _isSuspended;
        private double? _lastPreservedOldHeight;
        private double? _lastPreservedNewHeight;
        private int? _lastPreservedNewY;

        public FlowBlockNodeCenterPreservationGuard(FlowBlockNodeViewModel node)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
        }

        public bool IsSuspended => _isSuspended;

        public void SetSuspended(bool suspended)
            => _isSuspended = suspended;

        public void PreserveCenter(Action update)
        {
            var oldHeight = _node.Height;
            var centerY = _node.Y + oldHeight / 2d;

            update?.Invoke();

            var newHeight = _node.Height;
            if (Math.Abs(oldHeight - newHeight) < 0.1d)
            {
                ClearLastPreservedHeightChange();
                return;
            }

            var newY = Math.Max(0d, centerY - newHeight / 2d);
            if (IsDuplicateHeightPreservation(oldHeight, newHeight))
            {
                Log(
                    $"Node preserve center skipped duplicate, node={FormatNode()}, y={_node.Y:0.##}, calculatedY={newY:0.##}, oldHeight={oldHeight:0.##}, newHeight={newHeight:0.##}, centerY={centerY:0.##}, rows={_node.Rows.Count}");
                return;
            }

            if (Math.Abs(_node.Y - newY) > 0.1d)
            {
                Log(
                    $"Node preserve center, node={FormatNode()}, oldY={_node.Y:0.##}, newY={newY:0.##}, oldHeight={oldHeight:0.##}, newHeight={newHeight:0.##}, centerY={centerY:0.##}, rows={_node.Rows.Count}");
                _node.Y = newY;
            }

            RememberPreservedHeightChange(oldHeight, newHeight, newY);
        }

        public void Dispose()
        {
            _isSuspended = false;
            ClearLastPreservedHeightChange();
        }

        private bool IsDuplicateHeightPreservation(double oldHeight, double newHeight)
            => _lastPreservedOldHeight.HasValue &&
               _lastPreservedNewHeight.HasValue &&
               _lastPreservedNewY.HasValue &&
               Math.Abs(_lastPreservedOldHeight.Value - oldHeight) < 0.1d &&
               Math.Abs(_lastPreservedNewHeight.Value - newHeight) < 0.1d &&
               Math.Abs(_node.Y - _lastPreservedNewY.Value) < 0.1d;

        private void RememberPreservedHeightChange(double oldHeight, double newHeight, double newY)
        {
            _lastPreservedOldHeight = oldHeight;
            _lastPreservedNewHeight = newHeight;
            _lastPreservedNewY = (int)Math.Round(newY);
        }

        private void ClearLastPreservedHeightChange()
        {
            _lastPreservedOldHeight = null;
            _lastPreservedNewHeight = null;
            _lastPreservedNewY = null;
        }

        private string FormatNode()
            => $"{_node.Name} [{_node.InternalFlowBlock.GetType().Name}]";

        private static void Log(string message)
            => Trace.TraceInformation($"ProjectPanel layout trace: {message}");
    }
}