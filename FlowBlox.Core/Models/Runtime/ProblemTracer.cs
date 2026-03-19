using FlowBlox.Core.Util;
using Newtonsoft.Json;

namespace FlowBlox.Core.Models.Runtime
{
    public class ProblemsTracer
    {
        private readonly string _problemTraceDir;
        private readonly string _baseTraceFilePath;
        private string _traceFilePath;
        private int _traceFileIndex;
        private ProblemTraceSummary _traceSummary;
        private readonly int _entryLimit = 500;

        public ProblemsTracer(BaseRuntime runtime)
        {
            _problemTraceDir = FlowBloxOptions.GetOptionInstance().GetOption("Paths.ProblemTraceDir").Value;
            
            if (runtime is not FlowBloxRuntime fBRuntime)
                throw new NotSupportedException($"ProblemsTracer only supports instances of {nameof(FlowBloxRuntime)}.");

            _baseTraceFilePath = Path.Combine(
                _problemTraceDir,
                $"{Path.GetFileNameWithoutExtension(fBRuntime.RuntimeLogFileName)}_problems.json");
            
            _traceSummary = new ProblemTraceSummary();
            _traceFilePath = _baseTraceFilePath;
            _traceFileIndex = 0;
        }

        public void AppendTrace(ProblemTrace trace)
        {
            if (_traceSummary.Traces.Count >= _entryLimit)
                RollTraceFile();

            _traceSummary.Traces.Add(trace);
            SerializeTraceSummary();
        }

        private void RollTraceFile()
        {
            _traceFileIndex++;
            _traceFilePath = $"{Path.GetFileNameWithoutExtension(_baseTraceFilePath)}_{_traceFileIndex}.json";
            _traceSummary = new ProblemTraceSummary();
        }

        private void SerializeTraceSummary()
        {
            if (!Directory.Exists(_problemTraceDir))
                Directory.CreateDirectory(_problemTraceDir);

            var json = JsonConvert.SerializeObject(_traceSummary, Formatting.Indented);
            File.WriteAllText(_traceFilePath, json);
        }
    }
}
