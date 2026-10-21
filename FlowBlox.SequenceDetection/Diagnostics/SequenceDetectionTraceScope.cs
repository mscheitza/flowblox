using System;
using System.Collections.Generic;
using System.Text;

namespace FlowBlox.SequenceDetection.Diagnostics
{
    internal sealed class SequenceDetectionTraceScope : IDisposable
    {
        private readonly SequenceDetectionTraceContext _context;
        private readonly SequenceDetectionTraceContext? _previousContext;
        private bool _disposed;

        public SequenceDetectionTraceScope(SequenceDetectionTraceContext context, SequenceDetectionTraceContext? previousContext)
        {
            _context = context;
            _previousContext = previousContext;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _context.Disable();
            SequenceDetectionTraceContext.CurrentContext.Value = _previousContext;
            _disposed = true;
        }
    }
}
