namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBlockToolboxRegistrationService
    {
        IEnumerable<string> GetAllToolboxResourcesInModule();

        IEnumerable<Models.Components.FlowBloxToolboxCategoryItem> GetAllToolboxCategoriesInModule();

        void Register();
    }
}
