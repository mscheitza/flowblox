using FlowBlox.Core.Actions;

namespace FlowBlox.UICore.Models
{
    public class ProjectChangelist
    {
        public List<FlowBloxBaseAction> Changes { get; }

        public int ChangeIndex { get; private set; } = -1;

        public bool CanUndo => ChangeIndex > -1;

        public bool CanRedo => ChangeIndex < Changes.Count - 1;

        public event EventHandler Changed;
        public event EventHandler<ProjectChangelistActionEventArgs> BeforeUndo;
        public event EventHandler<ProjectChangelistActionEventArgs> AfterUndo;
        public event EventHandler<ProjectChangelistActionEventArgs> BeforeRedo;
        public event EventHandler<ProjectChangelistActionEventArgs> AfterRedo;

        public ProjectChangelist()
        {
            Changes = new List<FlowBloxBaseAction>();
        }

        public void ClearChanges()
        {
            ChangeIndex = -1;
            Changes.Clear();
            OnChanged();
        }

        public void AddChange(FlowBloxBaseAction action)
        {
            if (action == null)
                return;

            var removeAt = ChangeIndex + 1;
            if (ChangeIndex < Changes.Count - 1)
                Changes.RemoveRange(removeAt, Changes.Count - removeAt);

            Changes.Add(action);
            ChangeIndex++;
            OnChanged();
        }

        public FlowBloxBaseAction Undo()
        {
            if (!CanUndo)
                return null;

            var action = Changes[ChangeIndex];
            OnBeforeUndo(action, ChangeIndex);
            action.Undo();
            ChangeIndex--;
            OnAfterUndo(action, ChangeIndex);
            OnChanged();
            return action;
        }

        public FlowBloxBaseAction Redo()
        {
            if (!CanRedo)
                return null;

            ChangeIndex++;
            var action = Changes[ChangeIndex];
            OnBeforeRedo(action, ChangeIndex);
            action.Invoke();
            OnAfterRedo(action, ChangeIndex);
            OnChanged();
            return action;
        }

        private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);

        private void OnBeforeUndo(FlowBloxBaseAction action, int changeIndex)
            => BeforeUndo?.Invoke(this, new ProjectChangelistActionEventArgs(action, "Undo", changeIndex));

        private void OnAfterUndo(FlowBloxBaseAction action, int changeIndex)
            => AfterUndo?.Invoke(this, new ProjectChangelistActionEventArgs(action, "Undo", changeIndex));

        private void OnBeforeRedo(FlowBloxBaseAction action, int changeIndex)
            => BeforeRedo?.Invoke(this, new ProjectChangelistActionEventArgs(action, "Redo", changeIndex));

        private void OnAfterRedo(FlowBloxBaseAction action, int changeIndex)
            => AfterRedo?.Invoke(this, new ProjectChangelistActionEventArgs(action, "Redo", changeIndex));
    }

    public sealed class ProjectChangelistActionEventArgs : EventArgs
    {
        public ProjectChangelistActionEventArgs(FlowBloxBaseAction action, string operation, int changeIndex)
        {
            Action = action;
            Operation = operation;
            ChangeIndex = changeIndex;
        }

        public FlowBloxBaseAction Action { get; }
        public string Operation { get; }
        public int ChangeIndex { get; }
    }
}