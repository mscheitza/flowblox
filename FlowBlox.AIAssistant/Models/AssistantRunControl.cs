namespace FlowBlox.AIAssistant.Models
{
    /// <summary>Controls one active assistant run and queues user messages for its next model request.</summary>
    public sealed class AssistantRunControl
    {
        private readonly object _sync = new();
        private readonly Queue<string> _stagedUserMessages = new();
        private TaskCompletionSource? _resumeSource;
        private bool _isPauseRequested;
        private bool _isPaused;
        private bool _isCompleted;

        public event EventHandler? StateChanged;

        public bool IsPauseRequested
        {
            get
            {
                lock (_sync)
                    return _isPauseRequested;
            }
        }

        public bool IsPaused
        {
            get
            {
                lock (_sync)
                    return _isPaused;
            }
        }

        public void RequestPause()
        {
            var changed = false;
            lock (_sync)
            {
                if (!_isCompleted && !_isPauseRequested)
                {
                    _isPauseRequested = true;
                    changed = true;
                }
            }

            if (changed)
                StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Continue()
        {
            TaskCompletionSource? resumeSource;
            lock (_sync)
            {
                if (_isCompleted || (!_isPauseRequested && !_isPaused))
                    return;

                _isPauseRequested = false;
                _isPaused = false;
                resumeSource = _resumeSource;
                _resumeSource = null;
            }

            resumeSource?.TrySetResult();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task WaitWhilePausedAsync(CancellationToken cancellationToken)
        {
            Task? resumeTask = null;
            var changed = false;
            lock (_sync)
            {
                if (!_isCompleted && _isPauseRequested)
                {
                    _isPaused = true;
                    _resumeSource ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                    resumeTask = _resumeSource.Task;
                    changed = true;
                }
            }

            if (changed)
                StateChanged?.Invoke(this, EventArgs.Empty);

            if (resumeTask != null)
                await resumeTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        public bool TryStageUserMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            lock (_sync)
            {
                if (_isCompleted)
                    return false;

                _stagedUserMessages.Enqueue(message.Trim());
                return true;
            }
        }

        internal IReadOnlyList<string> DrainStagedUserMessages()
        {
            lock (_sync)
            {
                if (_stagedUserMessages.Count == 0)
                    return Array.Empty<string>();

                var messages = _stagedUserMessages.ToArray();
                _stagedUserMessages.Clear();
                return messages;
            }
        }

        internal bool TryCompleteWhenNoMessagesAreStaged()
        {
            lock (_sync)
            {
                if (_stagedUserMessages.Count != 0)
                    return false;

                _isCompleted = true;
                _isPauseRequested = false;
                _isPaused = false;
                return true;
            }
        }

        internal void Complete()
        {
            TaskCompletionSource? resumeSource;
            lock (_sync)
            {
                _isCompleted = true;
                _isPauseRequested = false;
                _isPaused = false;
                resumeSource = _resumeSource;
                _resumeSource = null;
            }

            resumeSource?.TrySetResult();
        }
    }
}