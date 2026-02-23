using FlowBlox.Core;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Drawing;
using FlowBlox.Core.Util.Resources;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

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

        private void ProjectView_Load(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void tbProjectName_TextChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Der Projektname im UI ist die Quelle für die Ableitung
            var name = tbProjectName.Text;

            var inputDir = _project.GetProjectInputDirectory(name);
            var outputDir = _project.GetProjectOutputDirectory(name);

            tbInputDir.Text = inputDir;
            tbOutputDir.Text = outputDir;

            var inputExists = !string.IsNullOrWhiteSpace(inputDir) && Directory.Exists(inputDir);
            var outputExists = !string.IsNullOrWhiteSpace(outputDir) && Directory.Exists(outputDir);

            btOpenInputDir.Enabled = inputExists;
            btOpenOutputDir.Enabled = outputExists;

            btCreateInputDir.Enabled = !string.IsNullOrWhiteSpace(inputDir) && !inputExists;
            btCreateOutputDir.Enabled = !string.IsNullOrWhiteSpace(outputDir) && !outputExists;
        }

        private void btCreateInputDir_Click(object sender, EventArgs e)
        {
            var dir = tbInputDir.Text;
            if (string.IsNullOrWhiteSpace(dir))
                return;

            Directory.CreateDirectory(dir);
            UpdateUI();
        }

        private void btCreateOutputDir_Click(object sender, EventArgs e)
        {
            var dir = tbOutputDir.Text;
            if (string.IsNullOrWhiteSpace(dir))
                return;

            Directory.CreateDirectory(dir);
            UpdateUI();
        }

        private void btOpenInputDir_Click(object sender, EventArgs e)
        {
            var dir = tbInputDir.Text;
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
        }

        private void btOpenOutputDir_Click(object sender, EventArgs e)
        {
            var dir = tbOutputDir.Text;
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
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
