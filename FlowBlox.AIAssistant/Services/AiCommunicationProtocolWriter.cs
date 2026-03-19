using System.Text;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    internal sealed class AiCommunicationProtocolWriter
    {
        private readonly string _directory;
        private readonly string _sessionId;
        private readonly string _userPrompt;
        private readonly DateTime _startedAtUtc;
        private readonly List<string> _lines = new();

        public AiCommunicationProtocolWriter(string directory, string sessionId, string userPrompt)
        {
            _directory = directory;
            _sessionId = sessionId;
            _userPrompt = userPrompt;
            _startedAtUtc = DateTime.UtcNow;

            _lines.Add("FlowBlox AI Assistant Communication Protocol");
            _lines.Add($"Started at (UTC): {_startedAtUtc:O}");
            _lines.Add($"Session ID: {_sessionId}");
            _lines.Add("User prompt:");
            _lines.Add(_userPrompt ?? string.Empty);
            _lines.Add(string.Empty);
        }

        public void AppendAiText(int round, string output)
        {
            AppendEntryHeader("AI", $"Round {round} output (text)");
            _lines.Add(output ?? string.Empty);
            _lines.Add(string.Empty);
        }

        public void AppendAiJson(int round, JObject? firstJsonObject)
        {
            AppendEntryHeader("AI", $"Round {round} output (first JSON object)");
            _lines.Add((firstJsonObject ?? new JObject()).ToString(Formatting.Indented));
            _lines.Add(string.Empty);
        }

        public void AppendAiAssistantServiceText(string title, string content)
        {
            AppendEntryHeader("AiAssistantService", title);
            _lines.Add(content ?? string.Empty);
            _lines.Add(string.Empty);
        }

        public void AppendToolCall(int round, ToolRequest request, ToolResponse response)
        {
            AppendEntryHeader("Tool API", $"Round {round} call '{request?.ToolName ?? string.Empty}'");
            _lines.Add($"Correlation ID: {request?.CorrelationId ?? string.Empty}");
            _lines.Add("Request arguments:");
            _lines.Add((request?.Arguments ?? new JObject()).ToString(Formatting.Indented));
            _lines.Add("Response:");
            _lines.Add((new JObject
            {
                ["ok"] = response?.Ok ?? false,
                ["error"] = response?.Error ?? string.Empty,
                ["result"] = response?.Result?.DeepClone() ?? new JObject(),
                ["log"] = response?.Log?.DeepClone() ?? new JArray()
            }).ToString(Formatting.Indented));
            _lines.Add(string.Empty);
        }

        public void TryWrite(ILogger? logger)
        {
            try
            {
                Directory.CreateDirectory(_directory);

                var fileName = $"ai_communication_protocol_{_startedAtUtc:yyyyMMdd_HHmmss_fff}.log";
                var filePath = Path.Combine(_directory, fileName);
                File.WriteAllText(filePath, string.Join(Environment.NewLine, _lines), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                logger?.Warn($"Failed to write AI communication protocol file: {ex.Message}");
            }
        }

        private void AppendEntryHeader(string source, string title)
        {
            _lines.Add($"[{DateTime.UtcNow:O}] {source} generated: {title}");
        }
    }
}
