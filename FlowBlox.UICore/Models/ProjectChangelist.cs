using FlowBlox.Core.Actions;

namespace FlowBlox.UICore.Models
{
    public class ProjectChangelist
    {
        public List<FlowBloxBaseAction> Changes { get; }

        public int ChangeIndex { get; set; } = -1;

        public ProjectChangelist()
        {
            Changes = new List<FlowBloxBaseAction>();
        }

        public void ClearChanges()
        {
            ChangeIndex = -1;
            Changes.Clear();
        }

        public void AddChange(FlowBloxBaseAction action)
        {
            var removeAt = ChangeIndex + 1;
            if (ChangeIndex < Changes.Count - 1)
                Changes.RemoveRange(removeAt, Changes.Count - removeAt);

            Changes.Add(action);
            ChangeIndex++;
        }
    }
}
