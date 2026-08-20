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
            action.Undo();
            ChangeIndex--;
            OnChanged();
            return action;
        }

        public FlowBloxBaseAction Redo()
        {
            if (!CanRedo)
                return null;

            ChangeIndex++;
            var action = Changes[ChangeIndex];
            action.Invoke();
            OnChanged();
            return action;
        }

        private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
    }
}
