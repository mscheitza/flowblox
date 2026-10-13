namespace FlowBlox.Core.Interfaces
{
    public interface IItemFactory<out T>
    {
        public T Create();
    }
}
