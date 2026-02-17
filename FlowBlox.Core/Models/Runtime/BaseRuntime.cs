using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Extensions;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.FlowBlocks;

namespace FlowBlox.Core.Models.Runtime
{
    public abstract partial class BaseRuntime : IDisposable
    {
        public delegate void LogMessageCreatedEventHandler(BaseRuntime runtime, string message, FlowBloxLogLevel logLevel);
        public delegate void FinishedEventHandler(object result);
        public delegate void RuntimeStartedEventHandler(BaseRuntime runtime);
        public delegate void FocusChangedEventHandler(BaseFlowBlock flowBlock);
        public delegate void PauseEventHandler(bool isPaused);

        public event LogMessageCreatedEventHandler LogMessageCreated;
        public event RuntimeStartedEventHandler RuntimeStarted;
        public event FinishedEventHandler Finish;
        public event FocusChangedEventHandler FocusChanged;
        public event PauseEventHandler PauseContinue;

        public bool Pause { get; set; }
        public bool Aborted { get; set; }
        public bool Running { get; set; }
        public bool AutoRestart { get; set; }
        public bool StepwiseExecution { get; set; }
        public bool StopOnWarning { get; set; }
        public bool StopOnError { get; set; }
        public int StepTimeunit { get; set; }
        public bool ExecutionFlowEnabled { get; set; }
        public bool DisableInterceptors { get; set; }

        private readonly IEnumerable<IRuntimeInterceptor> _interceptors;

        public IEnumerable<IRuntimeInterceptor> Interceptors => _interceptors;

        internal delegate void FieldChangeHandler(FieldElement fieldElement);

        internal event FieldChangeHandler FieldChanged;

        public DateTime Started { get; }

        private FlowBloxProject _project;
        public FlowBloxProject Project
        {
            get => _project;
            protected set
            {
                _project = value;

                foreach (var interceptor in _interceptors)
                {
                    interceptor.Project = _project;
                    interceptor.Runtime = this;
                }
            }
        }

        public BaseFlowBlock StartFlowBlock { get; protected set; }

        private RuntimeTaskRunner _runner;

        internal RuntimeTaskRunner TaskRunner => _runner ??= new RuntimeTaskRunner(this);

        protected BaseRuntime(FlowBloxProject project)
        {
            this.StartFlowBlock = FlowBloxRegistryProvider.GetRegistry().GetStartFlowBlock();

            if (StartFlowBlock == null)
                throw new Exception("You must define a valid start element.");

            _interceptors = FlowBloxServiceLocator.Instance.GetServices<IRuntimeInterceptor>();

            this.Project = project;
            this.ExecutionFlowEnabled = true;
            this.Started = DateTime.Now;
        }

        public virtual void Report(string message, FlowBloxLogLevel logLevel = FlowBloxLogLevel.Info, Exception e = null)
        {
            LogMessageCreated?.Invoke(this, message, logLevel);

            if (e != null)
            {
                LogMessageCreated?.Invoke(this, "The exception is logged below.", FlowBloxLogLevel.Error);
                LogMessageCreated?.Invoke(this, e.ToString(), FlowBloxLogLevel.Error);
            }
        }

        internal void NotifyFieldChange(FieldElement fieldElement)
        {
            FieldChanged?.Invoke(fieldElement);

            if (!DisableInterceptors)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.NotifyFieldChange(fieldElement);
                }
            }

            HandlePause();
        }

        protected void NotifyRuntimeStarted()
        {
            RuntimeStarted?.Invoke(this);

            if (!DisableInterceptors)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.NotifyRuntimeStarted();
                }
            }
        }

        protected void NotifyRuntimeFinished()
        {
            Finish?.Invoke(this);

            if (!DisableInterceptors)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.NotifyRuntimeFinished();
                }
            }
        }

        internal void NotifyInvocationStarted(BaseFlowBlock flowBlock)
        {
            Report($"Invocation started for flow block: \"{flowBlock.Name}\"", FlowBloxLogLevel.Info);

            if (!DisableInterceptors)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.NotifyInvocationStarted(flowBlock);
                }
            }
        }

        internal void NotifyInvocationFinished(BaseFlowBlock flowBlock)
        {
            if (!DisableInterceptors)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.NotifyInvocationFinished(flowBlock);
                }
            }

            Report($"Invocation finished for flow block: \"{flowBlock.Name}\"", FlowBloxLogLevel.Info);
        }

        public void NotifyWarning(BaseFlowBlock baseFlowBlock, string message)
        {
            if (StopOnWarning)
                Pause = true;

            if (!DisableInterceptors)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.NotifyWarning(baseFlowBlock, message);
                }
            }
        }

        public void NotifyError(BaseFlowBlock baseFlowBlock, string message, Exception exception = null)
        {
            if (StopOnError)
                Pause = true;

            if (!DisableInterceptors)
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.NotifyError(baseFlowBlock, message, exception);
                }
            }
        }

        public void Focus(BaseFlowBlock baseGridElement)
        {
            if (FocusChanged != null)
                FocusChanged(baseGridElement);
        }

        protected void RaisePauseContinue(bool pause)
        {
            PauseContinue?.Invoke(pause);
        }

        private readonly HashSet<BaseFlowBlock> _initializedFlowBlocks = new HashSet<BaseFlowBlock>();

        private void InitializeFlowBlock(BaseFlowBlock flowBlock)
        {
            if (flowBlock.ReferencedFlowBlocks.All(refBlock => _initializedFlowBlocks.Contains(refBlock)))
            {
                if (!_initializedFlowBlocks.Contains(flowBlock))
                {
                    flowBlock.RuntimeStarted(this);
                    _initializedFlowBlocks.Add(flowBlock);
                    flowBlock.GetNextFlowBlocks().ForEach(x => InitializeFlowBlock(x));
                }
            }
        }

        protected virtual void OnBeforeRuntimeStarted(BaseFlowBlock startFlowBlock, IEnumerable<BaseFlowBlock> flowBlocks, IEnumerable<IManagedObject> managedObjects)
        {
            Report($"Initializing runtime for managed objects...");
            foreach (var managedObject in managedObjects)
            {
                managedObject.RuntimeStarted(this);
            }
            Report($"Initializing runtime for flow blocks...");
            InitializeFlowBlock(startFlowBlock);
            var notInitialized = flowBlocks.Except(flowBlocks.OfType<NoteFlowBlock>()).Where(x => !_initializedFlowBlocks.Contains(x));
            if (notInitialized.Any())
            {
                var notInitializedNames = string.Join(", ", notInitialized.Select(x => x.Name));
                throw new InvalidOperationException($"The following FlowBlocks have not been initialized: {notInitializedNames}. Please check their dependencies and ensure they are correctly configured to allow proper initialization.");
            }
            Report($"Runtime initialization completed.");
        }

        protected virtual void OnAfterRuntimeFinished(IEnumerable<BaseFlowBlock> flowBlocks, IEnumerable<IManagedObject> managedObjects)
        {
            Report($"Finishing runtime for managed objects...");
            foreach (var managedObject in managedObjects)
            {
                managedObject.RuntimeFinished(this);
            }
            Report($"Finishing runtime for flow blocks...");
            foreach (var flowBlock in flowBlocks)
            {
                flowBlock.RuntimeFinished(this);
            }
            Report($"Finishing runtime completed.");
        }

        private void IntegrityCheck(IEnumerable<BaseFlowBlock> flowBlocks, IEnumerable<FieldElement> userFields)
        {
            HashSet<FieldElement> fieldElements = new HashSet<FieldElement>();
            foreach (var resultFlowBlock in flowBlocks.OfType<BaseResultFlowBlock>())
            {
                fieldElements.AddRange(resultFlowBlock.Fields);
            }

            foreach (var flowBlock in flowBlocks)
            {
                foreach (var dependendField in flowBlock.GetAssociatedFields())
                {
                    if (dependendField.UserField)
                    {
                        if (!userFields.Contains(dependendField))
                            throw new InvalidOperationException($"Referenced field \"{dependendField.FullyQualifiedName}\" used by \"{flowBlock.Name}\" could not be determined in the user field elements.");
                    }
                    else
                    {
                        if (!fieldElements.Contains(dependendField))
                            throw new InvalidOperationException($"Referenced field \"{dependendField.FullyQualifiedName}\" used by \"{flowBlock.Name}\" could not be determined in the field elements of the initialized FlowBlocks.");
                    }
                }
            }
        }

        private void ValidateFlowBlocks(IEnumerable<BaseFlowBlock> flowBlocks)
        {
            var validationResults = new List<ValidationResult>();
            var invalidBlocks = new Dictionary<BaseFlowBlock, List<ValidationResult>>();
            foreach (var block in flowBlocks)
            {
                var context = new ValidationContext(block, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();

                if (!Validator.TryValidateObject(block, context, results, true))
                {
                    invalidBlocks[block] = results;
                    validationResults.AddRange(results);
                }
            }

            if (validationResults.Any())
            {
                var errorMessage = "Validation errors occurred:";
                foreach (var kvp in invalidBlocks)
                {
                    errorMessage += $"\n\nFlowBlock: {kvp.Key.Name}";
                    foreach (var validationResult in kvp.Value)
                    {
                        errorMessage += $"\n  - {validationResult.ErrorMessage}";
                    }
                }

                throw new InvalidOperationException(errorMessage);
            }
        }

        protected virtual void OnBeforeRuntimeStarted(IEnumerable<BaseFlowBlock> flowBlocks)
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();

            var startFlowBlock = registry.GetStartFlowBlock();
            var managedObjects = registry.GetManagedObjects();
            var userFields = registry.GetUserFields();

            this.OnBeforeRuntimeStarted(startFlowBlock, flowBlocks, managedObjects);
            this.IntegrityCheck(flowBlocks, userFields);
            this.ValidateFlowBlocks(flowBlocks);
        }

        protected virtual void OnBeforeRuntimeStarted()
        {
            InitDefaultOptions();
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var flowBlocks = registry.GetFlowBlocks();
            OnBeforeRuntimeStarted(flowBlocks);
        }

        private void InitDefaultOptions()
        {
            var options = FlowBloxOptions.GetOptionInstance();
            options.InitDefaults(false);
        }

        protected virtual void OnAfterRuntimeFinished(IEnumerable<BaseFlowBlock> flowBlocks)
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var managedObjects = registry.GetManagedObjects();
            this.OnAfterRuntimeFinished(flowBlocks, managedObjects);
        }

        protected virtual void OnAfterRuntimeFinished()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var flowBlocks = registry.GetFlowBlocks();
            OnAfterRuntimeFinished(flowBlocks);
        }

        public virtual void HandlePause()
        {
            
        }

        public void Dispose()
        {

        }

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Returns a CancellationToken that can be used by FlowBlocks and external services.
        /// </summary>
        public virtual CancellationToken GetCancellationToken()
        {
            return _cancellationTokenSource.Token;
        }
    }
}