using FlowBlox.AIAssistant.Builder;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using Newtonsoft.Json;

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
        public void BuildChatRequest_KeepsLatestMessagePairWhenTokenBudgetIsTight()
        {
            var requestResult = AssistantChatRequestBuilder.Build(
                systemPrompt: "S",
                sessionBootstrapPrompt: "B",
                conversationSummary: string.Empty,
                sessionMessages:
                [
                    new AssistantMessagePair
                    {
                        AssistantRequest = "PAIR-ASSISTANT-REQUEST-TOO-LONG-FOR-BUDGET",
                        ToolApiResponse = "PAIR-TOOL-RESPONSE-FITS"
                    }
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

            Assert.AreEqual(1, historyMessages.Count);
            AssertContains(historyMessages[0].Content, "PAIR-ASSISTANT-REQUEST-TOO-LONG-FOR-BUDGET");
            AssertContains(historyMessages[0].Content, "PAIR-TOOL-RESPONSE-FITS");
            Assert.AreEqual("C", request.Messages.Last().Content);
        }

        [TestMethod]
        public void BuildChatRequest_SelectsCompleteSessionMessagesWhenMaxLatestMessagesIsReached()
        {
            var requestResult = AssistantChatRequestBuilder.Build(
                systemPrompt: "S",
                sessionBootstrapPrompt: "B",
                conversationSummary: string.Empty,
                sessionMessages:
                [
                    new AssistantSingleMessage { MessageRole = "user", Message = "SINGLE-1" },
                    new AssistantMessagePair { AssistantRequest = "PAIR-2-REQUEST", ToolApiResponse = "PAIR-2-RESPONSE" },
                    new AssistantMessagePair { AssistantRequest = "PAIR-3-REQUEST", ToolApiResponse = "PAIR-3-RESPONSE" }
                ],
                currentUserPrompt: "C",
                maxLatestMessages: 2,
                minLatestMessages: 1,
                tokenBudget: new AssistantTokenBudget
                {
                    MaxContextTokens = 1000,
                    ReservedResponseTokens = 0,
                    ApproximateCharactersPerToken = 4
                });

            var historyMessages = requestResult.Request.Messages.Take(requestResult.Request.Messages.Count - 1).ToList();

            Assert.AreEqual(2, historyMessages.Count);
            AssertContains(historyMessages[0].Content, "PAIR-2-REQUEST");
            AssertContains(historyMessages[0].Content, "PAIR-2-RESPONSE");
            AssertContains(historyMessages[1].Content, "PAIR-3-REQUEST");
            AssertContains(historyMessages[1].Content, "PAIR-3-RESPONSE");
            Assert.AreEqual(1, requestResult.FirstIncludedHistoryMessageIndex);
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
                [new AssistantMessagePair { AssistantRequest = "ASSISTANT-REQUEST-1", ToolApiResponse = "TOOL-RESPONSE-1" }]);

            var systemMessage = request.SystemMessages.Single().Content;

            AssertContains(systemMessage, "Goals");
            AssertContains(systemMessage, "Decisions");
            AssertContains(systemMessage, "Completed Changes");
            AssertContains(systemMessage, "Open Points");
            AssertContains(systemMessage, "Provider And Configuration Constraints");
            AssertContains(request.Messages.Single().Content, "ASSISTANT-REQUEST-1");
            AssertContains(request.Messages.Single().Content, "TOOL-RESPONSE-1");
            AssertContains(request.Messages.Single().Content, "MessagePair (AssistantRequest + ToolApiResponse):");
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

            Assert.AreEqual(3, history.SessionMessages.Count);
            var initialMessage = AssertIsInstanceOfType<AssistantSingleMessage>(history.SessionMessages[0]);
            Assert.AreEqual("user", initialMessage.Role);
            AssertContains(initialMessage.CompleteMessage, "User prompt:");
            AssertContains(initialMessage.CompleteMessage, "USER-TOOL-ROUND");

            var pair = AssertIsInstanceOfType<AssistantMessagePair>(history.SessionMessages[1]);
            AssertContains(pair.AssistantRequest, "GetProjectJson");
            AssertContains(pair.ToolApiResponse, "Tool execution updates since last assistant request:");
            AssertContains(pair.ToolApiResponse, "GetProjectJson");

            var finalMessage = AssertIsInstanceOfType<AssistantSingleMessage>(history.SessionMessages[2]);
            Assert.AreEqual("assistant", finalMessage.Role);
            AssertContains(finalMessage.CompleteMessage, "Done.");
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
                    new AssistantSingleMessage { MessageRole = "user", Message = "TECHNICAL-ROUND-PROMPT" },
                    new AssistantSingleMessage { MessageRole = "assistant", Message = "TECHNICAL-ASSISTANT-OUTPUT" }
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

        [TestMethod]
        public void HistoryDocument_RoundTripsPolymorphicSessionMessages()
        {
            var history = new AiAssistantHistoryDocument
            {
                SessionMessages =
                [
                    new AssistantSingleMessage { MessageRole = "user", Message = "USER-SINGLE" },
                    new AssistantMessagePair { AssistantRequest = "ASSISTANT-REQUEST", ToolApiResponse = "TOOL-RESPONSE" },
                    new AssistantSingleMessage { MessageRole = "assistant", Message = "ASSISTANT-FINAL" }
                ]
            };

            var json = JsonConvert.SerializeObject(history);
            var restored = JsonConvert.DeserializeObject<AiAssistantHistoryDocument>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(3, restored!.SessionMessages.Count);
            AssertIsInstanceOfType<AssistantSingleMessage>(restored.SessionMessages[0]);
            AssertIsInstanceOfType<AssistantMessagePair>(restored.SessionMessages[1]);
            AssertIsInstanceOfType<AssistantSingleMessage>(restored.SessionMessages[2]);
            AssertContains(restored.SessionMessages[1].CompleteMessage, "ASSISTANT-REQUEST");
            AssertContains(restored.SessionMessages[1].CompleteMessage, "TOOL-RESPONSE");
        }

        [TestMethod]
        public async Task GenerateProjectAsync_PersistsAssistantToolRequestWhenToolProcessingIsCanceled()
        {
            var executor = new QueuedAiExecutor(
                "{\"assistantMessage\":\"I will update the flow.\",\"final\":false,\"toolCalls\":[{\"toolName\":\"CancelingTool\",\"arguments\":{}}]}");
            var service = CreateService(
                executor,
                new CancelingToolApi(),
                CreateConfiguration(maxLatestMessages: 10));

            try
            {
                await service.GenerateProjectAsync("USER-CANCEL", CancellationToken.None);
                Assert.Fail("Expected OperationCanceledException.");
            }
            catch (OperationCanceledException)
            {
            }

            var history = new AiAssistantHistoryDocument();
            service.UpdateHistorySessionMetadata(history);

            Assert.AreEqual(2, history.SessionMessages.Count);
            AssertIsInstanceOfType<AssistantSingleMessage>(history.SessionMessages[0]);
            var assistantMessage = AssertIsInstanceOfType<AssistantSingleMessage>(history.SessionMessages[1]);
            Assert.AreEqual("assistant", assistantMessage.Role);
            AssertContains(assistantMessage.CompleteMessage, "CancelingTool");
        }

        [TestMethod]
        public async Task GenerateProjectAsync_PersistsNormalizedInstructionInsteadOfRawAssistantOutput()
        {
            var executor = new QueuedAiExecutor(
                "preface that should not be persisted {\"assistantMessage\":\"Reading project details.\",\"final\":false,\"toolCalls\":[{\"toolName\":\"GetProjectJson\",\"arguments\":{}}]} trailing text",
                "{\"assistantMessage\":\"Done.\",\"final\":true,\"toolCalls\":[]}");
            var config = CreateConfiguration(maxLatestMessages: 10);
            config.MaxToolRounds = 2;
            var service = CreateService(executor, config);

            await service.GenerateProjectAsync("USER-NORMALIZED", CancellationToken.None);

            var history = new AiAssistantHistoryDocument();
            service.UpdateHistorySessionMetadata(history);

            var pair = AssertIsInstanceOfType<AssistantMessagePair>(history.SessionMessages[1]);
            AssertContains(pair.AssistantRequest, "GetProjectJson");
            AssertDoesNotContain(pair.AssistantRequest, "preface that should not be persisted");
            AssertDoesNotContain(pair.AssistantRequest, "trailing text");
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

        private static AiAssistantService CreateService(
            IAiExecutor executor,
            IFlowBloxAIToolApi toolApi,
            AssistantConfiguration configuration)
        {
            return new AiAssistantService(
                executor,
                toolApi,
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

        private static T AssertIsInstanceOfType<T>(object value)
        {
            Assert.IsInstanceOfType(value, typeof(T));
            return (T)value;
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

        private sealed class CancelingToolApi : IFlowBloxAIToolApi
        {
            public event EventHandler<FlowBlocksConnectionsChangedEventArgs>? FlowBlocksConnectionsChanged
            {
                add { }
                remove { }
            }

            public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
            {
                throw new OperationCanceledException();
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
