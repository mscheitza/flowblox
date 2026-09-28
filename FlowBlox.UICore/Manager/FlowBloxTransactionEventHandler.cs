using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.UICore.Enums;
using MahApps.Metro.Controls;
using System.ComponentModel;

namespace FlowBlox.UICore.Manager
{
    /// <summary>
    /// Owns the complete lifecycle of a FlowBlox deep-copy transaction for editable views.
    /// Apply commits the current working copy and opens a fresh transaction from that committed state.
    /// </summary>
    public sealed class FlowBloxTransactionEventHandler
    {
        private readonly object _target;
        private readonly bool _deepCopy;
        private readonly bool _detached;
        private readonly bool _readOnly;
        private readonly bool _nested;
        private readonly Action<object> _workingCopyOpened;
        private readonly Func<bool> _canCommit;
        private readonly MetroWindow _window;
        private PropertyViewTransactionManager _transactionManager;

        public object WorkingCopy { get; private set; }

        public PropertyWindowCommitStatus CommitStatus { get; private set; }

        public bool HasActiveTransaction =>
            _deepCopy ? _transactionManager != null : WorkingCopy != null;

        public FlowBloxTransactionEventHandler(
            object target,
            bool deepCopy = true,
            bool detached = false,
            bool readOnly = false,
            bool nested = false,
            Action<object> workingCopyOpened = null,
            Func<bool> canCommit = null,
            MetroWindow window = null)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _deepCopy = deepCopy;
            _detached = detached;
            _readOnly = readOnly;
            _nested = nested;
            _workingCopyOpened = workingCopyOpened;
            _canCommit = canCommit;
            _window = window;
        }

        public object Open()
        {
            if (_transactionManager != null || WorkingCopy != null)
                throw new InvalidOperationException("The transaction has already been opened.");

            return OpenWorkingCopy();
        }

        private object Reopen()
        {
            if (_transactionManager != null)
                throw new InvalidOperationException("The current transaction must be committed before it can be reopened.");

            WorkingCopy = null;
            return OpenWorkingCopy();
        }

        private object OpenWorkingCopy()
        {
            try
            {
                if (_deepCopy)
                {
                    _transactionManager = new PropertyViewTransactionManager();
                    WorkingCopy = _transactionManager.Open(_target, _detached, _nested).TransientTarget;
                }
                else
                {
                    WorkingCopy = _target;
                }

                if (_deepCopy && !_readOnly && WorkingCopy is FlowBloxComponent component)
                    component.OnAfterOpen();

                _workingCopyOpened?.Invoke(WorkingCopy);
                return WorkingCopy;
            }
            catch
            {
                CancelActiveTransaction();
                throw;
            }
        }

        public object Append(object sourceObject)
        {
            if (!_deepCopy || _transactionManager == null)
                throw new InvalidOperationException("No deep-copy transaction is active.");

            return _transactionManager.Append(sourceObject);
        }

        public bool Save()
        {
            if (!CanCommit())
                return false;

            Commit(PropertyWindowCommitStatus.Saved);

            if (_window != null)
                _window.DialogResult = true;

            return true;
        }

        public bool Apply()
        {
            if (!CanCommit())
                return false;

            Commit(PropertyWindowCommitStatus.Applied);
            Reopen();
            return true;
        }

        public void Cancel()
        {
            CancelActiveTransaction();

            if (_window != null)
                _window.DialogResult = CommitStatus == PropertyWindowCommitStatus.None ? false : true;
        }

        public void Rollback() => CancelActiveTransaction();

        public void CloseEmbeddedTransaction() => Rollback();

        public void HandleClosing(object sender, CancelEventArgs e)
        {
            CancelActiveTransaction();

            var window = _window ?? sender as MetroWindow;
            if (window?.DialogResult != true && CommitStatus != PropertyWindowCommitStatus.None)
                window.DialogResult = true;
        }

        private bool CanCommit() => !_readOnly && (_canCommit?.Invoke() ?? true);

        private void Commit(PropertyWindowCommitStatus commitStatus)
        {
            if (WorkingCopy is IFlowBloxComponent transientComponent)
                transientComponent.OnBeforeSave();

            if (_deepCopy)
            {
                if (_transactionManager == null)
                    throw new InvalidOperationException("No deep-copy transaction is active.");

                _transactionManager.Commit(_target, WorkingCopy);
                _transactionManager = null;
            }

            CommitStatus = commitStatus;

            if (_target is IFlowBloxComponent component)
                component.OnAfterSave();
        }

        private void CancelActiveTransaction()
        {
            if (_deepCopy)
                _transactionManager?.Cancel();

            _transactionManager = null;
            WorkingCopy = null;
        }
    }
}
