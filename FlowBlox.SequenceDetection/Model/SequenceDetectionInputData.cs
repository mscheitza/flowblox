using FlowBlox.SequenceDetection.Constants;

namespace FlowBlox.SequenceDetection.Model
{
    public class SequenceDetectionInputData
    {
        public SequenceDetectionInputData()
        {
            Timeout = SequenceDetectionConstants.DefaultSequenceDetectionScannerTimeout;
        }

        public List<SequenceDetectionInputEntry>? Entries { get; set; }

        public int Timeout { get; set; }
    }
}
