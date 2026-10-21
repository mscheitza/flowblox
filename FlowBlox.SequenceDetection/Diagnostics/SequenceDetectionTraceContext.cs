using System;
using System.Collections.Generic;
using System.Text;

namespace FlowBlox.SequenceDetection.Diagnostics
{
    internal sealed class SequenceDetectionTraceContext
    {
        internal static readonly AsyncLocal<SequenceDetectionTraceContext?> CurrentContext = new();

        private readonly Action<string> _writer;
        private int _enabled = 1;

        public SequenceDetectionTraceContext(Action<string> writer)
        {
            _writer = writer;
        }

        public void Write(string message)
        {
            if (Volatile.Read(ref _enabled) == 1)
                _writer(message);
        }

        public void Disable()
        {
            Volatile.Write(ref _enabled, 0);
        }
    }
}
