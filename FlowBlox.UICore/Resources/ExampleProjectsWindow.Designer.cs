namespace FlowBlox.UICore.Resources
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ExampleProjectsWindow
    {
        private static global::System.Resources.ResourceManager? resourceMan;
        private static global::System.Globalization.CultureInfo? resourceCulture;

        internal ExampleProjectsWindow()
        {
        }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager =>
            resourceMan ??= new global::System.Resources.ResourceManager(
                "FlowBlox.UICore.Resources.ExampleProjectsWindow",
                typeof(ExampleProjectsWindow).Assembly);

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo? Culture
        {
            get => resourceCulture;
            set => resourceCulture = value;
        }

        public static string Button_Back => ResourceManager.GetString("Button_Back", resourceCulture)!;
        public static string Button_Open => ResourceManager.GetString("Button_Open", resourceCulture)!;
        public static string Header_Description => ResourceManager.GetString("Header_Description", resourceCulture)!;
        public static string Header_Title => ResourceManager.GetString("Header_Title", resourceCulture)!;
        public static string Title => ResourceManager.GetString("Title", resourceCulture)!;
    }
}
