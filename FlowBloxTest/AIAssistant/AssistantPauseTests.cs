using FlowBlox.AIAssistant.Builder;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant;

[TestClass]
[TestCategory(FlowBloxTestCategories.UnitTest)]
public sealed class AssistantPauseTests
{
    private const string ToolRound = """
        {"assistantMessage":"Working","final":false,"toolCalls":[
          {"toolName":"Record","arguments":{"id":1}},
          {"toolName":"Record","arguments":{"id":2}}
        ]}
        """;
    private const string FinalRound = """
        {"assistantMessage":"Updated using your new message.","final":true,"toolCalls":[]}
        """;

    [TestMethod]
    public async Task PauseDuringAiRequest_FinishesResponseAndAllToolsWithoutCancellation()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var control = new AssistantRunControl();
        var executor = new DelegateExecutor(async (
            _,
            ct) =>
        {
            entered.SetResult();
            await release.Task.WaitAsync(ct);
            return new()
            {
                Success = true,
                OutputText = ToolRound
            };
        });
        var tools = new RecordingToolApi();
        var service = CreateService(executor, tools);
        using var cancellation = new CancellationTokenSource();
        var run = service.GenerateProjectAsync(
            "Build something",
            cancellation.Token,
            control);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        control.RequestPause();
        release.SetResult();
        var result = await run;

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.IsPaused);
        Assert.IsFalse(cancellation.IsCancellationRequested);
        CollectionAssert.AreEqual(new[] { 1,
            2 }, tools.ExecutedIds);
        Assert.AreEqual(1, executor.Requests.Count);
        var pair = (AssistantMessagePair)service.GetDebugSnapshot().History.Last().Message;
        StringAssert.Contains(pair.ToolApiResponse, "TOOL-2");
        Assert.IsFalse(JObject.Parse(pair.AssistantRequest).Value<bool>("final"));
        service.ResetSession();
    }

    [TestMethod]
    public async Task PauseDuringToolBatch_SendAndContinueAppendsNewUserMessageAfterToolResults()
    {
        const string originalMessage = "Erstelle eine rote Tabelle.";
        const string insertedMessage = "Bitte blau statt rot.";

        var control = new AssistantRunControl();
        var executor = new DelegateExecutor((
            _,
            _) => Task.FromResult(new AiExecutorResult
        {
            Success = true,
            OutputText = ToolRound
        }));
        var tools = new RecordingToolApi
        {
            OnExecute = _ => control.RequestPause()
        };
        var service = CreateService(executor, tools);
        // Also verifies that pause on the last configured round succeeds instead of reporting a round-limit failure.
        var first = await service.GenerateProjectAsync(
            originalMessage,
            CancellationToken.None,
            control);
        Assert.IsTrue(first.Success);
        Assert.IsTrue(first.IsPaused);
        Assert.AreEqual(0, first.Errors.Count);
        Assert.AreEqual(2, tools.ExecutedIds.Count);
        executor.Handle = (_, _) => Task.FromResult(new AiExecutorResult
        {
            Success = true,
            OutputText = FinalRound
        });
        var second = await service.GenerateProjectAsync(
            insertedMessage,
            CancellationToken.None,
            new AssistantRunControl());
        Assert.IsTrue(second.Success);
        Assert.IsFalse(second.IsPaused);
        Assert.AreEqual(2, executor.Requests.Count);
        var resumedMessages = executor.Requests.Last().Messages.ToList();
        var originalMessageIndex = resumedMessages.FindIndex(message =>
            message.Role == "user" && message.Content.Contains(originalMessage, StringComparison.Ordinal));
        var toolResultIndex = resumedMessages.FindLastIndex(message => message.Role == "tool");
        var insertedMessages = resumedMessages
            .Where(message => message.Content.Contains(insertedMessage, StringComparison.Ordinal))
            .ToList();

        Assert.IsTrue(originalMessageIndex >= 0, "The original task must remain in the resumed request.");
        Assert.IsTrue(toolResultIndex > originalMessageIndex);
        var toolContext = string.Join("\n", resumedMessages.Where(message => message.Role == "tool")
            .Select(message => message.Content));
        StringAssert.Contains(toolContext, "TOOL-1");
        StringAssert.Contains(toolContext, "TOOL-2");
        Assert.AreEqual(1, insertedMessages.Count, "The inserted message must be sent exactly once.");
        Assert.AreEqual("user", insertedMessages[0].Role);
        Assert.AreSame(resumedMessages.Last(), insertedMessages[0]);
        Assert.IsTrue(resumedMessages.IndexOf(insertedMessages[0]) > toolResultIndex);

        var persistedInsertedMessages = service.GetDebugSnapshot().History
            .Where(entry => entry.Message.Role == "user" &&
                entry.Message.CompleteMessage.Contains(insertedMessage, StringComparison.Ordinal))
            .ToList();
        Assert.AreEqual(1, persistedInsertedMessages.Count,
            "The inserted message must remain in session history after resuming.");
        CollectionAssert.AreEqual(new[] { 1, 2 }, tools.ExecutedIds,
            "Resuming must not execute the completed tool calls again.");
        service.ResetSession();
    }

    [TestMethod]
    public async Task PauseWithMalformedResponse_PreservesResponseWithoutStartingFormatRetry()
    {
        var control = new AssistantRunControl();
        var executor = new DelegateExecutor((
            _,
            _) =>
        {
            control.RequestPause();
            return Task.FromResult(new AiExecutorResult
            {
                Success = true,
                OutputText = "MALFORMED RESPONSE"
            });
        });
        var service = CreateService(executor, new RecordingToolApi());
        var result = await service.GenerateProjectAsync(
            "Request",
            CancellationToken.None,
            control);
        Assert.IsTrue(result.IsPaused);
        Assert.AreEqual(1, executor.Requests.Count);
        StringAssert.Contains(service.GetDebugSnapshot().History.Last().Message.CompleteMessage, "MALFORMED RESPONSE");
        service.ResetSession();
    }

    [TestMethod]
    public async Task StopAfterPauseRequest_StillCancelsToolExecution()
    {
        var control = new AssistantRunControl();
        using var cancellation = new CancellationTokenSource();
        var executor = new DelegateExecutor((_, _) => Task.FromResult(new AiExecutorResult
        {
            Success = true,
            OutputText = ToolRound
        }));
        var tools = new RecordingToolApi
        {
            OnExecute = _ => { control.RequestPause(); cancellation.Cancel(); }
        };
        var service = CreateService(executor, tools);
        try
        {
            await service.GenerateProjectAsync(
                "Request",
                cancellation.Token,
                control);
            Assert.Fail("Stop must still cancel a pause-requested run.");
        }
        catch (OperationCanceledException) { }

        Assert.AreEqual(1, tools.ExecutedIds.Count);
        service.ResetSession();
    }

    private static AiAssistantService CreateService(
        IAiExecutor executor,
        IFlowBloxAIToolApi tools)
    {
        FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
        return new AiAssistantService(
            executor,
            tools,
            configurationProvider: () => new AssistantConfiguration
            {
                MaxToolRounds = 1,
                MaxLatestMessages = 10,
                MinLatestMessages = 1,
                MaxContextTokens = 100000,
                EnableAutomaticAdjustment = false
            });
    }

    private sealed class DelegateExecutor(Func<AIChatRequest, CancellationToken, Task<AiExecutorResult>> handle) : IAiExecutor
    {
        public Func<AIChatRequest, CancellationToken, Task<AiExecutorResult>> Handle { get; set; } = handle;
        public List<AIChatRequest> Requests { get; } = new();

        public Task<AiExecutorResult> ExecuteChatAsync(
            AIChatRequest request,
            AssistantConfiguration configuration,
            CancellationToken ct)
        {
            Requests.Add(request);
            return Handle(request, ct);
        }
    }

    private sealed class RecordingToolApi : IFlowBloxAIToolApi
    {
        public List<int> ExecutedIds { get; } = new();
        public Action<int>? OnExecute
        {
        get; init;
        }

        public IReadOnlyList<ToolDefinition> GetToolDefinitions() => [];

        public Task<ToolResponse> ExecuteAsync(
            ToolRequest request,
            CancellationToken ct)
        {
            var id = request.Arguments.Value<int>("id");
            ExecutedIds.Add(id);
            OnExecute?.Invoke(id);
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new ToolResponse
            {
                Ok = true,
                Result = new JObject
                {
                    ["marker"] = "TOOL-" + id
                }
            });
        }
    }
}
