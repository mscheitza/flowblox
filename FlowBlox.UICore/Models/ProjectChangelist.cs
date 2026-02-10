using FlowBlox.UICore.Actions;

namespace FlowBlox.UICore.Models
{
    public class ProjectChangelist
    {
        public List<FlowBloxBaseAction> Changes { get; }

        public int ChangeIndex { get; set; } = -1;

        public ProjectChangelist()
        {
            this.Changes = new List<FlowBloxBaseAction>();
        }

        public void ClearChanges()
        {
            ChangeIndex = -1;
            Changes.Clear();
        }

        public void AddChange(FlowBloxBaseAction action)
        {
            var removeAt = ChangeIndex + 1;
            if (ChangeIndex < this.Changes.Count - 1)
                this.Changes.RemoveRange(removeAt, this.Changes.Count - removeAt);

            this.Changes.Add(action);

            ChangeIndex++;
        }
    }
}
