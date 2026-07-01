using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Util;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace FlowBlox.AIAssistant.History
{
    public sealed class AiAssistantHistoryStore
    {
        private const string OptionName = "AI.AssistantHistoryDirectory";
        private readonly object _sync = new();
        private bool _isInitialized;

        public static AiAssistantHistoryStore Instance { get; } = new();

        public ObservableCollection<AiAssistantHistoryListItem> Histories { get; } = new();

        public string GetHistoryDirectory()
        {
            var options = FlowBloxOptions.GetOptionInstance();
            var directory = options.GetOption(OptionName)?.Value;
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "FlowBlox",
                    "ai_assistant_histories");
            }

            directory = Environment.ExpandEnvironmentVariables(directory);
            Directory.CreateDirectory(directory);
            return directory;
        }

        public void Initialize(Guid? projectGuid = null)
        {
            lock (_sync)
            {
                if (_isInitialized)
                    return;

                _isInitialized = true;
            }

            Refresh(projectGuid);
        }

        public void Refresh(Guid? projectGuid = null)
        {
            var items = LoadList(projectGuid);
            Histories.Clear();
            foreach (var item in items)
                Histories.Add(item);
        }

        public IReadOnlyList<AiAssistantHistoryListItem> LoadList(Guid? projectGuid = null)
        {
            var directory = GetHistoryDirectory();
            return Directory.EnumerateFiles(directory, "history_*.json")
                .Select(TryLoadListItem)
                .Where(x => x != null)
                .Select(x => x!)
                .Where(x => projectGuid == null || x.ProjectGuid == projectGuid.Value)
                .OrderByDescending(x => x.UpdatedAt)
                .ToList();
        }

        public AiAssistantHistoryDocument? Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            var document = JsonConvert.DeserializeObject<AiAssistantHistoryDocument>(File.ReadAllText(filePath));
            if (document != null && document.HistoryGuid == Guid.Empty)
                document.HistoryGuid = Guid.NewGuid();

            return document;
        }

        public string CreateOrUpdateHistory(AiAssistantHistoryDocument document, string? filePath)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.HistoryGuid == Guid.Empty)
                document.HistoryGuid = Guid.NewGuid();

            if (document.CreatedAt == default)
                document.CreatedAt = DateTimeOffset.Now;

            document.UpdatedAt = DateTimeOffset.Now;

            if (string.IsNullOrWhiteSpace(filePath))
                filePath = Histories.FirstOrDefault(x => x.HistoryGuid == document.HistoryGuid)?.FilePath;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = Path.Combine(
                    GetHistoryDirectory(),
                    $"history_{document.CreatedAt:yyyyMMdd_HHmmss_fff}.json");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(document, Formatting.Indented));
            UpsertListItem(ToListItem(document, filePath));
            return filePath;
        }

        private void UpsertListItem(AiAssistantHistoryListItem item)
        {
            var existing = Histories.FirstOrDefault(x => x.HistoryGuid == item.HistoryGuid);
            if (existing != null)
            {
                var oldIndex = Histories.IndexOf(existing);
                if (oldIndex >= 0)
                    Histories.RemoveAt(oldIndex);
            }

            InsertSorted(item);
        }

        private void InsertSorted(AiAssistantHistoryListItem item)
        {
            var insertIndex = 0;
            while (insertIndex < Histories.Count && Histories[insertIndex].UpdatedAt > item.UpdatedAt)
                insertIndex++;

            Histories.Insert(insertIndex, item);
        }

        private static AiAssistantHistoryListItem? TryLoadListItem(string filePath)
        {
            try
            {
                var document = JsonConvert.DeserializeObject<AiAssistantHistoryDocument>(File.ReadAllText(filePath));
                if (document == null)
                    return null;

                if (document.HistoryGuid == Guid.Empty)
                    document.HistoryGuid = Guid.NewGuid();

                return ToListItem(document, filePath);
            }
            catch
            {
                return null;
            }
        }

        private static AiAssistantHistoryListItem ToListItem(AiAssistantHistoryDocument document, string filePath)
        {
            return new AiAssistantHistoryListItem
            {
                HistoryGuid = document.HistoryGuid,
                FilePath = filePath,
                ProjectGuid = document.ProjectGuid,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                LastRound = document.LastRound,
                Preview = document.Transcripts
                    .LastOrDefault(x => x.Kind == AssistantTranscriptKind.User || x.Kind == AssistantTranscriptKind.Assistant)
                    ?.Text ?? string.Empty
            };
        }
    }
}
