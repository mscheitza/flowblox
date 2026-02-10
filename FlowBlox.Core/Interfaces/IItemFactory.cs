namespace FlowBlox.Core.Interfaces
{
    public interface IItemFactory<out T> where T : IFlowBloxComponent
    {
        public T Create();
    }
}
