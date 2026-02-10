namespace FlowBlox.Core.Interfaces
{
    public interface IDockableObjectManager : IObjectManager
    {
        bool IsActive { get; }
    }
}
