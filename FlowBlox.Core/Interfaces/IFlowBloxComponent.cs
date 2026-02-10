using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Runtime;
using System.Collections.ObjectModel;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBloxComponent
    {
        string Name { get; set; }

        bool HandleRequirements { get; }

        ObservableCollection<FieldElement> RequiredFields { get; set; }

        IEnumerable<IManagedObject> GetAssociatedManagedObjects();

        IEnumerable<FieldElement> GetAssociatedFields();

        bool IsDeletable(out List<IFlowBloxComponent> dependencies);

        void OnAfterCreate();

        void OnAfterLoad();

        void RegisterPropertyChangedEventHandlers();

        void OnBeforeSave();

        void OnAfterSave();

        void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions);

        void RuntimeStarted(BaseRuntime runtime);

        void RuntimeFinished(BaseRuntime runtime);
    }
}
