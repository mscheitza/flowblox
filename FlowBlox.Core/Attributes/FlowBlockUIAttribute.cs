namespace FlowBlox.Core.Attributes
{
    [Flags()]
    public enum UIOperations
    {
        None = 0,
        Create = 1,
        Edit = 2,
        Delete = 4,
        Link = 8,
        Unlink = 16,
        All = 31
    }

    public enum UIFactory
    {
        Default,
        GridView,
        ListView,
        Association,
        ListViewSplitMode,
        ComboBox
    }

    [Flags]
    public enum UIOptions
    {
        /// <summary>
        /// Enables field/placeholder selection for text-like editors.
        /// </summary>
        EnableFieldSelection = 1,

        /// <summary>
        /// Enables file selection button for text-like editors.
        /// </summary>
        EnableFileSelection = 2,

        /// <summary>
        /// Enables folder selection button for text-like editors.
        /// </summary>
        EnableFolderSelection = 4
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FlowBlockUIAttribute : Attribute
    {
        public FlowBlockUIAttribute()
        {
            this.DisplayLabel = true;
            this.Operations = UIOperations.All;
            this.Visible = true;
            this.CreatableTypes = Array.Empty<Type>();
        }

        public UIOperations Operations { get; set; }

        public bool DisplayLabel { get; set; }

        public bool Visible { get; set; }

        public UIFactory Factory { get; set; }

        public string ToolboxCategory { get; set; }

        public string SelectionFilterMethod { get; set; }

        public string ReadOnlyMethod { get; set; }

        public UIOptions UiOptions { get; set; }
        
        public string SelectionDisplayMember { get; set; }
        
        public int Width { get; set; }
        public int Height { get; set; }
        
        public bool ReadOnly { get; set; }

        // <summary>
        /// If set, only these concrete types may be created for abstract/interface property types.
        /// </summary>
        public Type[] CreatableTypes { get; set; }
    }
}
