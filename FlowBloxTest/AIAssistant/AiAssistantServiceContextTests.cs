using FlowBlox.AIAssistant.Builder;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    public class AiAssistantServiceContextTests
    {
        [TestMethod]
        public async Task GenerateProjectAsync_SummarizesOnlyMessagesLeavingLatestWindow()
        {
            var executor = new RecordingAiExecutor();
            var service = CreateService(executor, CreateConfiguration(maxLatestMessages: 2));

            await service.GenerateProjectAsync("USER-1", CancellationToken.None);
            await service.GenerateProjectAsync("USER-2", CancellationToken.None);
            await service.GenerateProjectAsync("USER-3", CancellationToken.None);

            var summaryRequests = executor.Requests
                .Where(x => x.Source == "FlowBloxAIAssistantSummary")
                .ToList();

            Assert.AreEqual(1, summaryRequests.Count);
            AssertContains(summaryRequests[0].Messages.Single().Content, "USER-1");
            AssertContains(summaryRequests[0].Messages.Single().Content, "ASSISTANT-USER-1");
            AssertDoesNotContain(summaryRequests[0].Messages.Single().Content, "USER-2");
        }

        [TestMethod]
        public async Task GenerateProjectAsync_PreservesAllPriorMessagesThroughSummaryAndLatestWindow()
        {
            var executor = new RecordingAiExecutor();
            var service = CreateService(executor, CreateConfiguration(maxLatestMessages: 2));

            await service.GenerateProjectAsync("USER-1", CancellationToken.None);
            await service.GenerateProjectAsync("USER-2", CancellationToken.None);
            await service.GenerateProjectAsync("USER-3", CancellationToken.None);

            var thirdChatRequest = executor.ChatRequests[2];
            var combinedContext = string.Join(
                "\n",
                thirdChatRequest.SystemMessages.Select(x => x.Content).Concat(thirdChatRequest.Messages.Select(x => x.Content)));

            AssertContains(combinedContext, "USER-1");
            AssertContains(combinedContext, "ASSISTANT-USER-1");
            AssertContains(combinedContext, "USER-2");
            AssertContains(combinedContext, "ASSISTANT-USER-2");
            AssertContains(combinedContext, "USER-3");
        }

        [TestMethod]
        public void BuildChatRequest_KeepsLatestPairWhenTokenBudgetIsTight()
        {
            const string pairId = "PAIR-1";
            var requestResult = AssistantChatRequestBuilder.Build(
                systemPrompt: "S",
                sessionBootstrapPrompt: "B",
                conversationSummary: string.Empty,
                sessionMessages:
                [
                    new AssistantConversationMessage { Role = "user", PairId = pairId, Content = "PAIR-USER-TOO-LONG-FOR-BUDGET" },
                    new AssistantConversationMessage { Role = "assistant", PairId = pairId, Content = "PAIR-ASSISTANT-FITS" }
                ],
                currentUserPrompt: "C",
                maxLatestMessages: 5,
                minLatestMessages: 1,
                tokenBudget: new AssistantTokenBudget
                {
                    MaxContextTokens = 14,
                    ReservedResponseTokens = 0,
                    ApproximateCharactersPerToken = 1
                });

            var request = requestResult.Request;
            var historyMessages = request.Messages.Take(request.Messages.Count - 1).ToList();

            Assert.AreEqual(2, historyMessages.Count);
            Assert.AreEqual("PAIR-USER-TOO-LONG-FOR-BUDGET", historyMessages[0].Content);
            Assert.AreEqual("PAIR-ASSISTANT-FITS", historyMessages[1].Content);
            Assert.AreEqual("C", request.Messages.Last().Content);
        }

        [TestMethod]
        public void BuildChatRequest_DoesNotSplitOlderPairWhenMaxLatestMessagesIsReached()
        {
            var requestResult = AssistantChatRequestBuilder.Build(
                systemPrompt: "S",
                sessionBootstrapPrompt: "B",
                conversationSummary: string.Empty,
                sessionMessages:
                [
                    new AssistantConversationMessage { Role = "user", PairId = "PAIR-1", Content = "PAIR-1-USER" },
                    new AssistantConversationMessage { Role = "assistant", PairId = "PAIR-1", Content = "PAIR-1-ASSISTANT" },
                    new AssistantConversationMessage { Role = "user", PairId = "PAIR-2", Content = "PAIR-2-USER" },
                    new AssistantConversationMessage { Role = "assistant", PairId = "PAIR-2", Content = "PAIR-2-ASSISTANT" },
                    new AssistantConversationMessage { Role = "user", PairId = "PAIR-3", Content = "PAIR-3-USER" },
                    new AssistantConversationMessage { Role = "assistant", PairId = "PAIR-3", Content = "PAIR-3-ASSISTANT" }
                ],
                currentUserPrompt: "C",
                maxLatestMessages: 4,
                minLatestMessages: 2,
                tokenBudget: new AssistantTokenBudget
                {
                    MaxContextTokens = 1000,
                    ReservedResponseTokens = 0,
                    ApproximateCharactersPerToken = 4
                });

            var historyMessages = requestResult.Request.Messages.Take(requestResult.Request.Messages.Count - 1).ToList();

            Assert.AreEqual(4, historyMessages.Count);
            Assert.AreEqual("PAIR-2-USER", historyMessages[0].Content);
            Assert.AreEqual("PAIR-2-ASSISTANT", historyMessages[1].Content);
            Assert.AreEqual("PAIR-3-USER", historyMessages[2].Content);
            Assert.AreEqual("PAIR-3-ASSISTANT", historyMessages[3].Content);
            Assert.AreEqual(2, requestResult.FirstIncludedHistoryMessageIndex);
        }

        [TestMethod]
        public void BuildChatRequest_MarksOnlyStableSystemMessagesForCaching()
        {
            var requestResult = AssistantChatRequestBuilder.Build(
                systemPrompt: "Stable system",
                sessionBootstrapPrompt: "Stable bootstrap",
                conversationSummary: "Variable summary",
                sessionMessages: [],
                currentUserPrompt: "Current prompt",
                maxLatestMessages: 5,
                minLatestMessages: 1,
                tokenBudget: new AssistantTokenBudget
                {
                    MaxContextTokens = 1000,
                    ReservedResponseTokens = 0,
                    ApproximateCharactersPerToken = 4
                });

            var request = requestResult.Request;

            Assert.AreEqual(AIChatCacheBehavior.PreferCache, request.SystemMessages[0].CacheBehavior);
            Assert.AreEqual(AIChatCacheBehavior.PreferCache, request.SystemMessages[1].CacheBehavior);
            Assert.AreEqual(AIChatCacheBehavior.Default, request.SystemMessages[2].CacheBehavior);
        }


        [TestMethod]
        public async Task GenerateProjectAsync_SummarizesMessagesExcludedByEffectiveTokenWindow()
        {
            var executor = new RecordingAiExecutor();
            var config = CreateConfiguration(maxLatestMessages: 4);
            config.MinLatestMessages = 1;
            config.MaxContextTokens = 35;
            config.ApproximateCharactersPerToken = 1;
            var service = CreateService(executor, config);

            await service.GenerateProjectAsync("USER-1-LONG", CancellationToken.None);
            await service.GenerateProjectAsync("USER-2-LONG", CancellationToken.None);
            await service.GenerateProjectAsync("USER-3-LONG", CancellationToken.None);

            var thirdChatRequest = executor.ChatRequests[2];
            var combinedContext = string.Join(
                "\n",
                thirdChatRequest.SystemMessages.Select(x => x.Content).Concat(thirdChatRequest.Messages.Select(x => x.Content)));

            AssertContains(combinedContext, "USER-1-LONG");
            AssertContains(combinedContext, "ASSISTANT-USER-1-LONG");
            AssertContains(combinedContext, "USER-2-LONG");
            AssertContains(combinedContext, "ASSISTANT-USER-2-LONG");
            AssertContains(combinedContext, "USER-3-LONG");
        }
        [TestMethod]
        public void BuildSummaryRequest_UsesStructuredSummaryContract()
        {
            var request = AssistantSummaryRequestBuilder.Build(
                "## Goals\n- Existing goal",
                [new AssistantConversationMessage { Role = "user", Source = "ToolApiResultPrompt", Content = "USER-1" }]);

            var systemMessage = request.SystemMessages.Single().Content;

            AssertContains(systemMessage, "Goals");
            AssertContains(systemMessage, "Decisions");
            AssertContains(systemMessage, "Completed Changes");
            AssertContains(systemMessage, "Open Points");
            AssertContains(systemMessage, "Provider And Configuration Constraints");
            AssertContains(request.Messages.Single().Content, "USER-1");
            AssertContains(request.Messages.Single().Content, "User (ToolApiResultPrompt):");
        }

        [TestMethod]
        public async Task GenerateProjectAsync_PersistsTechnicalSessionMessagesIncludingToolRounds()
        {
            var executor = new QueuedAiExecutor(
                "{\"assistantMessage\":\"Reading project details.\",\"final\":false,\"toolCalls\":[{\"toolName\":\"GetProjectJson\",\"arguments\":{}}]}",
                "{\"assistantMessage\":\"Done.\",\"final\":true,\"toolCalls\":[]}");
            var config = CreateConfiguration(maxLatestMessages: 10);
            config.MaxToolRounds = 2;
            var service = CreateService(executor, config);

            await service.GenerateProjectAsync("USER-TOOL-ROUND", CancellationToken.None);

            var history = new AiAssistantHistoryDocument();
            service.UpdateHistorySessionMetadata(history);

            Assert.AreEqual(4, history.SessionMessages.Count);
            Assert.AreEqual("user", history.SessionMessages[0].Role);
            Assert.AreEqual("UserPrompt", history.SessionMessages[0].Source);
            Assert.IsFalse(string.IsNullOrWhiteSpace(history.SessionMessages[0].PairId));
            AssertContains(history.SessionMessages[0].Content, "User prompt:");
            AssertContains(history.SessionMessages[0].Content, "USER-TOOL-ROUND");
            Assert.AreEqual("assistant", history.SessionMessages[1].Role);
            Assert.AreEqual("AssistantResponse", history.SessionMessages[1].Source);
            Assert.AreEqual(history.SessionMessages[0].PairId, history.SessionMessages[1].PairId);
            AssertContains(history.SessionMessages[1].Content, "GetProjectJson");
            Assert.AreEqual("user", history.SessionMessages[2].Role);
            Assert.AreEqual("ToolApiResultPrompt", history.SessionMessages[2].Source);
            Assert.IsFalse(string.IsNullOrWhiteSpace(history.SessionMessages[2].PairId));
            Assert.AreNotEqual(history.SessionMessages[0].PairId, history.SessionMessages[2].PairId);
            AssertContains(history.SessionMessages[2].Content, "Tool execution updates since last round:");
            AssertContains(history.SessionMessages[2].Content, "GetProjectJson");
            Assert.AreEqual("assistant", history.SessionMessages[3].Role);
            Assert.AreEqual("AssistantResponse", history.SessionMessages[3].Source);
            Assert.AreEqual(history.SessionMessages[2].PairId, history.SessionMessages[3].PairId);
            AssertContains(history.SessionMessages[3].Content, "Done.");
        }

        [TestMethod]
        public async Task RestoreSession_UsesPersistedSessionMessagesBeforeTranscriptFallback()
        {
            var executor = new RecordingAiExecutor();
            var service = CreateService(executor, CreateConfiguration(maxLatestMessages: 10));
            service.RestoreSession(new AiAssistantHistoryDocument
            {
                SessionMessages =
                [
                    new AssistantConversationMessage { Role = "user", Content = "TECHNICAL-ROUND-PROMPT" },
                    new AssistantConversationMessage { Role = "assistant", Content = "TECHNICAL-ASSISTANT-OUTPUT" }
                ],
                Transcripts =
                [
                    new AssistantTranscriptLine { Kind = AssistantTranscriptKind.User, Text = "UI-ONLY-USER" },
                    new AssistantTranscriptLine { Kind = AssistantTranscriptKind.Assistant, Text = "UI-ONLY-ASSISTANT" }
                ]
            });

            await service.GenerateProjectAsync("NEXT-USER", CancellationToken.None);

            var combinedContext = string.Join(
                "\n",
                executor.ChatRequests.Single().Messages.Select(x => x.Content));

            AssertContains(combinedContext, "TECHNICAL-ROUND-PROMPT");
            AssertContains(combinedContext, "TECHNICAL-ASSISTANT-OUTPUT");
            AssertDoesNotContain(combinedContext, "UI-ONLY-USER");
            AssertDoesNotContain(combinedContext, "UI-ONLY-ASSISTANT");
        }

        private static AiAssistantService CreateService(RecordingAiExecutor executor, AssistantConfiguration configuration)
        {
            return new AiAssistantService(
                executor,
                new EmptyToolApi(),
                configurationProvider: () => configuration);
        }

        private static AiAssistantService CreateService(IAiExecutor executor, AssistantConfiguration configuration)
        {
            return new AiAssistantService(
                executor,
                new EmptyToolApi(),
                configurationProvider: () => configuration);
        }

        private static AssistantConfiguration CreateConfiguration(int maxLatestMessages)
        {
            return new AssistantConfiguration
            {
                MaxToolRounds = 1,
                MaxLatestMessages = maxLatestMessages,
                MaxContextTokens = 100000,
                ReservedResponseTokens = 0,
                ApproximateCharactersPerToken = 4,
                MinLatestMessages = 1,
                EnableAutomaticAdjustment = false
            };
        }

        private static void AssertContains(string actual, string expected)
        {
            StringAssert.Contains(actual, expected);
        }

        private static void AssertDoesNotContain(string actual, string unexpected)
        {
            Assert.IsFalse(actual.Contains(unexpected, StringComparison.Ordinal), $"Did not expect '{unexpected}' in:\n{actual}");
        }

        private sealed class EmptyToolApi : IFlowBloxAIToolApi
        {
            public event EventHandler<FlowBlocksConnectionsChangedEventArgs>? FlowBlocksConnectionsChanged
            {
                add { }
                remove { }
            }

            public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
            {
                return Task.FromResult(new ToolResponse { Ok = true });
            }

            public IReadOnlyList<ToolDefinition> GetToolDefinitions()
            {
                return Array.Empty<ToolDefinition>();
            }
        }

        private sealed class RecordingAiExecutor : IAiExecutor
        {
            public List<AIChatRequest> Requests { get; } = new();
            public List<AIChatRequest> ChatRequests => Requests
                .Where(x => x.Source != "FlowBloxAIAssistantSummary")
                .ToList();

            public Task<AiExecutorResult> ExecuteChatAsync(
                AIChatRequest request,
                AssistantConfiguration configuration,
                CancellationToken ct)
            {
                Requests.Add(request);

                if (request.Source == "FlowBloxAIAssistantSummary")
                {
                    return Task.FromResult(new AiExecutorResult
                    {
                        Success = true,
                        OutputText = "[SUMMARIZED]\n" + request.Messages.Single().Content
                    });
                }

                var userPrompt = ExtractUserPrompt(request.Messages.Last().Content);
                return Task.FromResult(new AiExecutorResult
                {
                    Success = true,
                    OutputText = "{\"assistantMessage\":\"ASSISTANT-" + userPrompt + "\",\"final\":true,\"toolCalls\":[]}"
                });
            }

            private static string ExtractUserPrompt(string roundPrompt)
            {
                var marker = "User prompt:";
                var markerIndex = roundPrompt.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex < 0)
                    return roundPrompt.Trim();

                var afterMarker = roundPrompt[(markerIndex + marker.Length)..].TrimStart();
                using var reader = new StringReader(afterMarker);
                return reader.ReadLine()?.Trim() ?? string.Empty;
            }
        }

        private sealed class QueuedAiExecutor : IAiExecutor
        {
            private readonly Queue<string> _responses;

            public QueuedAiExecutor(params string[] responses)
            {
                _responses = new Queue<string>(responses ?? Array.Empty<string>());
            }

            public Task<AiExecutorResult> ExecuteChatAsync(
                AIChatRequest request,
                AssistantConfiguration configuration,
                CancellationToken ct)
            {
                if (request.Source == "FlowBloxAIAssistantSummary")
                {
                    return Task.FromResult(new AiExecutorResult
                    {
                        Success = true,
                        OutputText = "[SUMMARIZED]\n" + request.Messages.Single().Content
                    });
                }

                return Task.FromResult(new AiExecutorResult
                {
                    Success = true,
                    OutputText = _responses.Count == 0
                        ? "{\"assistantMessage\":\"Done.\",\"final\":true,\"toolCalls\":[]}"
                        : _responses.Dequeue()
                });
            }
        }
    }
}
