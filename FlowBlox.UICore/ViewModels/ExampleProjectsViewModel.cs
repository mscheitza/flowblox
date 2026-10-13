using FlowBlox.Core.Logging;
using FlowBlox.Core.Util.DistributedData;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Views;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class ExampleProjectsViewModel : INotifyPropertyChanged
    {
        private readonly ExampleProjectsWindow _window;
        private ExampleProjectItem? _selectedProject;

        public ObservableCollection<ExampleProjectItem> Projects { get; } = new();
        public RelayCommand BackCommand { get; }
        public RelayCommand OpenCommand { get; }

        public ExampleProjectItem? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (ReferenceEquals(_selectedProject, value))
                    return;

                _selectedProject = value;
                OnPropertyChanged();
                OpenCommand.Invalidate();
            }
        }

        public ExampleProjectsViewModel(ExampleProjectsWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            BackCommand = new RelayCommand(() => _window.DialogResult = false);
            OpenCommand = new RelayCommand(OpenSelectedProject, () => SelectedProject != null);
            LoadProjects();
        }

        private void LoadProjects()
        {
            var dataType = DistributedDataCatalog.GetDataTypes()
                .FirstOrDefault(x => string.Equals(
                    x.Name,
                    DistributedDataCatalog.ExampleProjectType,
                    StringComparison.OrdinalIgnoreCase));
            if (dataType == null)
                return;

            foreach (var fileName in dataType.FileNames)
            {
                var projectFile = DistributedDataCatalog.ResolvePath(dataType.Name, fileName);
                if (string.IsNullOrWhiteSpace(projectFile))
                    continue;

                try
                {
                    var projectJson = JObject.Parse(File.ReadAllText(projectFile));
                    Projects.Add(new ExampleProjectItem(
                        projectJson.Value<string>("ProjectName")?.Trim() ?? Path.GetFileNameWithoutExtension(fileName),
                        projectJson.Value<string>("ProjectDescription")?.Trim() ?? string.Empty,
                        File.GetCreationTime(projectFile),
                        projectFile));
                }
                catch (Exception ex)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error(
                        $"Failed to read example project metadata from '{projectFile}'.",
                        ex);
                }
            }

            var orderedProjects = Projects
                .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            Projects.Clear();
            foreach (var project in orderedProjects)
                Projects.Add(project);
        }

        private void OpenSelectedProject()
        {
            if (SelectedProject == null)
                return;

            _window.Tag = SelectedProject.ProjectFile;
            _window.DialogResult = true;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public sealed record ExampleProjectItem(
        string Name,
        string Description,
        DateTime CreatedAt,
        string ProjectFile);
}
