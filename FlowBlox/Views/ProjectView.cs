using System;
using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Util.Drawing;

namespace FlowBlox.Views
{
    
    public enum ProjectViewMode
    {
        CreateProject,
        EditProject
    }

    public partial class ProjectView : Form
    {
        private FlowBloxProject _project;
        private ProjectViewMode _mode;

        public ProjectView(FlowBloxProject project, ProjectViewMode mode)
        {
            InitializeComponent();
            FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);

            _project = project;
            _mode = mode;

            this.tbProjectName.Text = project.ProjectName;
            this.tbProjectDescription.Text = project.ProjectDescription;
            this.tbAuthor.Text = project.Author;
            this.tbNoteOnOpening.Text = project.Notice;

            ApplyMode();
        }

        private void ApplyMode()
        {
            switch (_mode)
            {
                case ProjectViewMode.CreateProject:
                    this.pictureBox.Image = FlowBloxMainUIImages.create_48;
                    this.Text = FlowBloxResourceUtil.GetLocalizedString("ProjectView_CreateProject_Text", typeof(FlowBloxMainUITexts));
                    this.Icon = ImageHelper.ConvertImageToIcon(FlowBloxMainUIImages.create_16);
                    break;
                case ProjectViewMode.EditProject:
                    this.pictureBox.Image = FlowBloxMainUIImages.edit_48;
                    this.Text = FlowBloxResourceUtil.GetLocalizedString("ProjectView_EditProject_Text", typeof(FlowBloxMainUITexts));
                    this.Icon = ImageHelper.ConvertImageToIcon(FlowBloxMainUIImages.edit_16);
                    break;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbProjectName.Text))
            {
                FlowBloxMessageBox.Show(this, "Bitte geben Sie eine gültige Projektbezeichnung an.", "Ungültige Projekt Eigenschaften");
                return;
            }

            _project.ProjectName = tbProjectName.Text;
            _project.Author = tbAuthor.Text;
            _project.ProjectDescription = tbProjectDescription.Text;
            _project.Notice = tbNoteOnOpening.Text;

            this.Close();
        }
    }
}
