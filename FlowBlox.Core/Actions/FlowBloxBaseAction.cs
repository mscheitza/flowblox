namespace FlowBlox.Core.Actions
{
    public abstract class FlowBloxBaseAction
    {
        protected FlowBloxBaseAction()
        {
            AssociatedActions = new List<FlowBloxBaseAction>();
        }

        public List<FlowBloxBaseAction> AssociatedActions { get; internal set; }

        public virtual void Undo()
        {
            AssociatedActions.ForEach(x => x.Undo());
        }

        public virtual void Invoke()
        {
            AssociatedActions.ForEach(x => x.Invoke());
        }
    }
}
