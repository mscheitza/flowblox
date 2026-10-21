namespace FlowBlox.AIAssistant.Models;

/// <summary>One control per submitted prompt. Pausing finishes the current round without cancellation.</summary>
public sealed class AssistantRunControl
{
    private int _pauseRequested;
    public bool IsPauseRequested => Volatile.Read(ref _pauseRequested) != 0;

    public void RequestPause() => Interlocked.Exchange(ref _pauseRequested, 1);
}
