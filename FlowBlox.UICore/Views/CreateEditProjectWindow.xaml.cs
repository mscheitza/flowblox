using FlowBlox.Core.Models.Project;
using FlowBlox.UICore.Rendering;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views;

public partial class CreateEditProjectWindow : MetroWindow
{
    public CreateEditProjectWindow(FlowBloxProject project, bool isEditing = false)
    {
        InitializeComponent();
        DescriptionInput.TextArea.TextView.BackgroundRenderers.Add(new SoftWrapContinuationRenderer(DescriptionInput));
        NoticeInput.TextArea.TextView.BackgroundRenderers.Add(new SoftWrapContinuationRenderer(NoticeInput));
        var model = new CreateEditProjectViewModel(project, isEditing, result => DialogResult = result);
        DataContext = model;
        Activated += (_, _) => model.RefreshDirectories();
        Loaded += (_, _) => ProjectNameInput.Focus();
    }
}
