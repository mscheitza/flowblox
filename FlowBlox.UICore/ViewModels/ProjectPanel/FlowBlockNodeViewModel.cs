using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels.ProjectPanel
{
    public sealed class FlowBlockNodeViewModel : INotifyPropertyChanged, IFlowBloxUIElement, IDisposable
    {
        internal const double FixedWidth = 328d;
        private const double HeaderHeight = 26d;
        private const double NoteHeight = 150d;
        private const double RowHeight = 24d;
        private const double NotificationHeight = 30d;
        internal const double MaxBlockHeight = 300d;

        private readonly SynchronizationContext _uiContext;
        private bool _isSelected;
        private bool _isReference;
        private bool _isRuntimeFocused;
        private string _warningMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private readonly FlowBlockNodeCenterPreservationGuard _centerPreservationGuard;
        private readonly FlowBloxComponentChangeSubscription _componentChangeSubscription;

        public FlowBlockNodeViewModel(BaseFlowBlock flowBlock)
        {
            _uiContext = SynchronizationContext.Current;
            InternalFlowBlock = flowBlock ?? throw new ArgumentNullException(nameof(flowBlock));
            _centerPreservationGuard = new FlowBlockNodeCenterPreservationGuard(this);
            _componentChangeSubscription = new FlowBloxComponentChangeSubscription(
                flowBlock,
                _ => FlowBlock_OnComponentChanged());
            flowBlock.PropertyChanged += FlowBlock_PropertyChanged;
            flowBlock.OnWarn += FlowBlock_OnWarn;
            flowBlock.OnError += FlowBlock_OnError;
            flowBlock.OnUndoWarn += FlowBlock_OnUndoWarn;
            flowBlock.OnUndoError += FlowBlock_OnUndoError;
            flowBlock.RefreshNotExecutedState();
            RefreshRows(preserveCenter: false);
        }

        public BaseFlowBlock InternalFlowBlock { get; }
        public ObservableCollection<FlowBlockRenderRowViewModel> Rows { get; } = new();
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ElementSelectedChangedByUser;

        public double X
        {
            get => InternalFlowBlock.Location.X;
            set
            {
                var x = Math.Max(0, (int)Math.Round(value));
                if (InternalFlowBlock.Location.X == x)
                    return;

                InternalFlowBlock.Location = new Point(x, InternalFlowBlock.Location.Y);
                OnPositionChanged();
            }
        }

        public double Y
        {
            get => InternalFlowBlock.Location.Y;
            set
            {
                var y = Math.Max(0, (int)Math.Round(value));
                if (InternalFlowBlock.Location.Y == y)
                    return;

                InternalFlowBlock.Location = new Point(InternalFlowBlock.Location.X, y);
                OnPositionChanged();
            }
        }

        public double Width => FixedWidth;
        public double Height => IsNote ? NoteHeight : Math.Min(MaxBlockHeight, DesiredHeight);
        public bool HasVerticalOverflow => !IsNote && DesiredHeight > MaxBlockHeight;
        public bool IsNote => InternalFlowBlock is NoteFlowBlock;
        public string NoteText => (InternalFlowBlock as NoteFlowBlock)?.Note ?? string.Empty;
        public string Title => FlowBloxComponentHelper.GetDisplayName(InternalFlowBlock);
        public string Name => InternalFlowBlock.Name;
        public bool HasBreakpoint => InternalFlowBlock.BreakPoint;
        public bool IsNotExecuted => InternalFlowBlock.IsNotExecuted;
        public bool HasExecutionIndex => InternalFlowBlock.ExecutionIndex >= 0;
        public string ExecutionIndexText => HasExecutionIndex ? $"#{InternalFlowBlock.ExecutionIndex}" : string.Empty;
        public bool HasNotification => HasWarning || HasError;
        public bool HasWarning => !string.IsNullOrWhiteSpace(_warningMessage);
        public bool HasError => !HasWarning && !string.IsNullOrWhiteSpace(_errorMessage);
        public string NotificationMessage => HasWarning ? _warningMessage : _errorMessage;
        private double RuntimeInfoHeight => HasExecutionIndex ? RowHeight : 0d;
        private double NotificationInfoHeight => HasNotification ? NotificationHeight : 0d;
        private double DesiredHeight => HeaderHeight + (Rows.Count * RowHeight) + RuntimeInfoHeight + NotificationInfoHeight + 8d;

        public bool ElementSelected => IsSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMarked));
                ElementSelectedChangedByUser?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsReference
        {
            get => _isReference;
            set
            {
                if (_isReference == value)
                    return;

                _isReference = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMarked));
            }
        }

        public bool IsMarked => IsSelected || IsReference;

        public bool IsRuntimeFocused
        {
            get => _isRuntimeFocused;
            set
            {
                if (_isRuntimeFocused == value)
                    return;

                _isRuntimeFocused = value;
                OnPropertyChanged();
            }
        }

        public void RefreshRows() => RefreshRows(preserveCenter: true);

        public void RefreshRowsWithoutCenterPreservation() => RefreshRows(preserveCenter: false);

        public void SetCenterPreservationSuspended(bool suspended)
            => _centerPreservationGuard.SetSuspended(suspended);

        private void RefreshRows(bool preserveCenter)
        {
            void Refresh()
            {
                Rows.Clear();
                if (IsNote)
                {
                    OnPropertyChanged(nameof(NoteText));
                }
                else
                {
                    RenderProperties();
                    RenderFields();
                    RenderRequiredFields();
                    RenderActivationConditions();
                }

                OnPropertyChanged(nameof(Rows));
                NotifyLayoutChanged();
                NotifyRuntimeStateChanged(preserveCenter: false);
            }

            if (preserveCenter && !_centerPreservationGuard.IsSuspended)
                UpdatePreservingCenter(Refresh);
            else
                Refresh();
        }

        public void NotifyRuntimeStateChanged() => NotifyRuntimeStateChanged(preserveCenter: true);

        private void NotifyRuntimeStateChanged(bool preserveCenter)
        {
            if (preserveCenter && !_centerPreservationGuard.IsSuspended)
            {
                UpdatePreservingCenter(() => NotifyRuntimeStateChanged(preserveCenter: false));
                return;
            }

            OnPropertyChanged(nameof(HasBreakpoint));
            OnPropertyChanged(nameof(HasExecutionIndex));
            OnPropertyChanged(nameof(ExecutionIndexText));
            NotifyLayoutChanged();
        }

        private void RenderProperties()
        {
            foreach (var propertyName in InternalFlowBlock.GetDisplayableProperties())
            {
                var property = InternalFlowBlock.GetType().GetProperty(propertyName);
                if (property == null)
                    continue;

                var propertyValue = FlowBloxFieldHelper.GetPropertyValueOrSelectedField(InternalFlowBlock, property);
                if (propertyValue == null)
                    continue;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType.IsGenericType)
                {
                    RenderListProperties(propertyName, propertyValue);
                    continue;
                }

                var displayValue = CleanValue(propertyValue.ToString());
                if (string.IsNullOrWhiteSpace(displayValue))
                    continue;

                var displayName = ResolveDisplayName(property, propertyName);
                Rows.Add(new FlowBlockRenderRowViewModel(
                    "property",
                    displayName,
                    displayValue,
                    propertyValue.ToString(),
                    InternalFlowBlock,
                    propertyName));
            }
        }

        private void RenderListProperties(string propertyName, object propertyValue)
        {
            var list = (System.Collections.IEnumerable)propertyValue;
            var listType = propertyValue.GetType().GetGenericArguments()[0];
            var displayName = ResolveDisplayName(listType, propertyName);
            var index = 0;

            foreach (var item in list)
            {
                var renderedProperties = new List<FlowBlockRenderRowViewModel>();
                foreach (var itemProperty in listType.GetProperties())
                {
                    var displayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();
                    if (displayAttribute == null)
                        continue;

                    var itemPropertyValue = itemProperty.GetValue(item);
                    if (itemPropertyValue == null)
                        continue;

                    var displayValue = CleanValue(itemPropertyValue.ToString());
                    if (string.IsNullOrWhiteSpace(displayValue))
                        continue;

                    var itemDisplayName = FlowBloxResourceUtil.GetDisplayName(displayAttribute, true);
                    renderedProperties.Add(new FlowBlockRenderRowViewModel(
                        "property",
                        itemDisplayName,
                        displayValue,
                        itemPropertyValue.ToString(),
                        InternalFlowBlock,
                        $"{propertyName}[{index}].{itemProperty.Name}",
                        item));
                }

                if (renderedProperties.Count > 0)
                {
                    Rows.Add(new FlowBlockRenderRowViewModel("header", $"{displayName} #{index + 1}", string.Empty));
                    foreach (var renderedProperty in renderedProperties)
                        Rows.Add(renderedProperty);
                }

                index++;
            }
        }

        private void RenderFields()
        {
            if (InternalFlowBlock is not BaseResultFlowBlock resultFlowBlock)
                return;

            foreach (var field in resultFlowBlock.Fields)
            {
                Rows.Add(new FlowBlockRenderRowViewModel("field", string.Empty, field.Name, field.FullyQualifiedName, field));
                foreach (var modifier in field.Modifiers)
                    Rows.Add(new FlowBlockRenderRowViewModel("modifier", "mod", CleanValue(modifier.ToString()), modifier.ToString(), modifier));
                foreach (var condition in field.Conditions)
                    Rows.Add(new FlowBlockRenderRowViewModel("condition", "cond", CleanValue(condition.ShortDisplayName), condition.DisplayName, condition));
            }
        }

        private void RenderRequiredFields()
        {
            foreach (var requiredFieldContext in InternalFlowBlock.GetRequiredFieldContexts())
            {
                Rows.Add(new FlowBlockRenderRowViewModel(
                    "required",
                    "req",
                    requiredFieldContext.FieldElement.Name,
                    requiredFieldContext.FieldElement.FullyQualifiedName,
                    requiredFieldContext.FlowBloxComponent,
                    nameof(FlowBloxComponent.RequiredFields),
                    requiredFieldContext.FieldElement));
            }
        }

        private void RenderActivationConditions()
        {
            foreach (var condition in InternalFlowBlock.ActivationConditions)
                Rows.Add(new FlowBlockRenderRowViewModel("activation", "if", CleanValue(condition.ShortDisplayName), condition.DisplayName, condition));
        }

        private static string ResolveDisplayName(MemberInfo member, string fallback)
        {
            var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute != null ? FlowBloxResourceUtil.GetDisplayName(displayAttribute, true) : fallback;
        }

        private static string CleanValue(string value)
            => string.IsNullOrEmpty(value) ? string.Empty : value.Replace(Environment.NewLine, " ").Trim();

        private void FlowBlock_OnComponentChanged()
            => SynchronizationContextHelper.PostToUi(_uiContext, RefreshRows);

        private void FlowBlock_OnWarn(BaseRuntime runtime, string message)
            => SynchronizationContextHelper.PostToUi(_uiContext, () => SetWarning(message));

        private void FlowBlock_OnError(BaseRuntime runtime, string message)
            => SynchronizationContextHelper.PostToUi(_uiContext, () => SetError(message));

        private void FlowBlock_OnUndoWarn(BaseRuntime runtime)
            => SynchronizationContextHelper.PostToUi(_uiContext, () => SetWarning(string.Empty));

        private void FlowBlock_OnUndoError(BaseRuntime runtime)
            => SynchronizationContextHelper.PostToUi(_uiContext, () => SetError(string.Empty));

        private void FlowBlock_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => SynchronizationContextHelper.PostToUi(_uiContext, () => HandleFlowBlockPropertyChanged(e));

        private void HandleFlowBlockPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BaseFlowBlock.Location))
                OnPositionChanged();
            else if (e.PropertyName == nameof(BaseFlowBlock.Name))
            {
                OnPropertyChanged(nameof(Name));
                RefreshRows();
            }
            else if (IsNote && e.PropertyName == nameof(NoteFlowBlock.Note))
                OnPropertyChanged(nameof(NoteText));
            else if (e.PropertyName == nameof(BaseFlowBlock.BreakPoint))
                NotifyRuntimeStateChanged();
            else if (e.PropertyName == nameof(BaseFlowBlock.IsNotExecuted))
                OnPropertyChanged(nameof(IsNotExecuted));
            else if (e.PropertyName == nameof(BaseFlowBlock.ExecutionIndex))
                NotifyRuntimeStateChanged();
            else if (!string.IsNullOrWhiteSpace(e.PropertyName))
                RefreshRows();
        }

        private void OnPositionChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        private void SetWarning(string message)
        {
            if (string.Equals(_warningMessage, message ?? string.Empty, StringComparison.Ordinal))
                return;

            UpdatePreservingCenter(() =>
            {
                _warningMessage = message ?? string.Empty;
                NotifyNotificationChanged();
            });
        }

        private void SetError(string message)
        {
            if (string.Equals(_errorMessage, message ?? string.Empty, StringComparison.Ordinal))
                return;

            UpdatePreservingCenter(() =>
            {
                _errorMessage = message ?? string.Empty;
                NotifyNotificationChanged();
            });
        }

        private void NotifyNotificationChanged()
        {
            OnPropertyChanged(nameof(HasNotification));
            OnPropertyChanged(nameof(HasWarning));
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(NotificationMessage));
            NotifyLayoutChanged();
        }

        private void NotifyLayoutChanged()
        {
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(HasVerticalOverflow));
        }

        private void UpdatePreservingCenter(Action update)
            => _centerPreservationGuard.PreserveCenter(update);

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void Dispose()
        {
            InternalFlowBlock.PropertyChanged -= FlowBlock_PropertyChanged;
            InternalFlowBlock.OnWarn -= FlowBlock_OnWarn;
            InternalFlowBlock.OnError -= FlowBlock_OnError;
            InternalFlowBlock.OnUndoWarn -= FlowBlock_OnUndoWarn;
            InternalFlowBlock.OnUndoError -= FlowBlock_OnUndoError;
            _componentChangeSubscription.Dispose();
            _centerPreservationGuard.Dispose();
        }
    }
}