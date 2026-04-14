using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components.IO;
using System.Collections.ObjectModel;
using FlowBlox.Core.Extensions;

namespace FlowBlox.Core.Models.ObjectManager
{
    [Display(Name = "DataObjectManager_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [UIMetadataDefinitions(typeof(FlowBloxIcons), nameof(FlowBloxIcons.database_cog), "#7C3AED", 16)]
    [FlowBloxUIGroup(Name = "DataObjectManager_Groups_FileObjects", ControlAlignment = ControlAlignment.Fill)]
    [FlowBloxUIGroup(Name = "DataObjectManager_Groups_MemoryObjects", ControlAlignment = ControlAlignment.Fill)]
    public class DataObjectManager : IDockableObjectManager
    {
        private FlowBloxRegistry _registry;

        public bool IsActive => true;

        public DataObjectManager()
        {
            FileObjects = new ObservableCollection<FileObject>();
            MemoryObjects = new ObservableCollection<MemoryObject>();
        }

        public DataObjectManager(FlowBloxRegistry registry) : this()
        {
            _registry = registry;

            Reload();
        }

        public void Reload()
        {
            FileObjects.Clear();
            FileObjects.AddRange(_registry.GetManagedObjects<FileObject>());

            MemoryObjects.Clear();
            MemoryObjects.AddRange(_registry.GetManagedObjects<MemoryObject>());
        }

        [Display(ResourceType = typeof(FlowBloxTexts), GroupName = "DataObjectManager_Groups_FileObjects")]
        [FlowBloxUI(Factory = UIFactory.ListView, DisplayLabel = false, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [FlowBloxListView(LVColumnMemberNames = new[] { nameof(FileObject.FilePath) })]
        public ObservableCollection<FileObject> FileObjects { get; set; }

        [Display(ResourceType = typeof(FlowBloxTexts), GroupName = "DataObjectManager_Groups_MemoryObjects")]
        [FlowBloxUI(Factory = UIFactory.ListView, DisplayLabel = false, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [FlowBloxListView(LVColumnMemberNames = new[] { nameof(MemoryObject.Field), nameof(MemoryObject.FileName) })]
        public ObservableCollection<MemoryObject> MemoryObjects { get; set; }
    }
}
