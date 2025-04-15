using FlowBlox.Core.Authentication;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Repository;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Validation;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class ExtensionsViewModel : INotifyPropertyChanged
    {
        private string _searchText;
        private FbExtensionResult _selectedExtension;
        private FbVersionResult _selectedVersion;
        private Window _ownerWindow;
        private FlowBloxProject _project;
        private bool _hasExtensionChanged;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public List<FbExtensionResult> SearchResults { get; set; }
        public FbExtensionResult SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                if (_selectedExtension != value)
                {
                    _selectedExtension = value;

                    if (_selectedExtension != null)
                    {
                        var installedExtension = this.InstalledExtensions.SingleOrDefault(x => x.ExtensionGuid == _selectedExtension.Guid);

                        SelectedVersion = _selectedExtension.Versions
                            .OrderBy(x => x.Version)
                            .LastOrDefault(x => installedExtension == null || x.Version == installedExtension.Version);
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsExtensionSelected));
                }
                
            }
        }

        public FbVersionResult SelectedVersion
        {
            get => _selectedVersion;
            set
            {
                if (_selectedVersion != value)
                {
                    _selectedVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsExtensionSelected => SelectedExtension != null;

        public ObservableCollection<FlowBloxProjectExtension> OfflineExtensions { get; set; }

        public ObservableCollection<FlowBloxProjectExtension> InstalledExtensions { get; set; }

        public ICommand AddExtensionCommand { get; private set; }
        public ICommand UpdateExtensionCommand { get; private set; }
        public ICommand RemoveExtensionCommand { get; private set; }
        public ICommand AddOfflineExtensionCommand { get; private set; }
        public ICommand RemoveOfflineExtensionCommand { get; private set; }
        public ICommand SelectFilePathCommand { get; private set; }
        public ICommand UninstallExtensionCommand { get; private set; }
        public RelayCommand SearchCommand { get; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand LoginCommand { get; private set; }
        public RelayCommand ManageOwnExtensionsCommand { get; }
        public ICommand LogoutCommand { get; private set; }
        public RelayCommand ReloadExtensionsCommand { get; private set; }

        public FbUserData ActiveUser
        {
            get => FlowBloxAccountManager.Instance.ActiveUser;
            set
            {
                FlowBloxAccountManager.Instance.ActiveUser = value;
                OnPropertyChanged(nameof(ActiveUser));
            }
        }

        public string UserToken
        {
            get => FlowBloxAccountManager.Instance.UserToken;
            set
            {
                FlowBloxAccountManager.Instance.UserToken = value;
                OnPropertyChanged(nameof(UserToken));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExtensionsViewModel(Window ownerWindow, FlowBloxProject project) : this()
        {
            _ownerWindow = ownerWindow;
            _project = project;

            LoadExtensions(project);

            InstalledExtensions.CollectionChanged += (s, e) =>
            {
                _hasExtensionChanged = true;
                CommandManager.InvalidateRequerySuggested(); // Aktualisiere Command CanExecute Status
            };
        }

        public ExtensionsViewModel()
        {
            SearchCommand = new RelayCommand(ExecuteSearch);

            RegisterCommand = new RelayCommand(() =>
            {
                var registrationWindow = new RegistrationWindow
                {
                    Owner = this._ownerWindow
                };
                registrationWindow.ShowDialog();
            });

            LoginCommand = new RelayCommand(ExecuteLogin);
            ManageOwnExtensionsCommand = new RelayCommand(ManageOwnExtensions);

            LogoutCommand = new RelayCommand(() =>
            {
                ActiveUser = null;
                UserToken = null;
            });
            

            // Initialisierung der ObservableCollections
            OfflineExtensions = new ObservableCollection<FlowBloxProjectExtension>();
            InstalledExtensions = new ObservableCollection<FlowBloxProjectExtension>();

            // Initialisierung der Commands
            AddExtensionCommand = new RelayCommand(AddExtension, CanAddExtension);
            UpdateExtensionCommand = new RelayCommand(UpdateExtension, CanUpdateExtension);
            RemoveExtensionCommand = new RelayCommand(RemoveExtension, CanRemoveExtension);

            AddOfflineExtensionCommand = new RelayCommand(AddOfflineExtension);
            RemoveOfflineExtensionCommand = new RelayCommand(RemoveOfflineExtension, CanModifyExtension);
            SelectFilePathCommand = new RelayCommand(SelectFilePath, CanModifyExtension);
            
            UninstallExtensionCommand = new RelayCommand(UninstallExtension, CanUninstallExtension);
            ReloadExtensionsCommand = new RelayCommand(ReloadExtensions, CanReloadExtensions);
        }

        private void ManageOwnExtensions(object obj)
        {
            var manageUserExtensionsWindow = new ManageUserExtensionsWindow(
                FlowBloxAccountManager.Instance.UserToken, 
                FlowBloxAccountManager.Instance.ActiveUser)
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            manageUserExtensionsWindow.ShowDialog();
        }

        private Lazy<FlowBloxWebApiService> _flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
        {
            var webApiServiceUrl = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ExtensionApiServiceBaseUrl"].Value;
            return new FlowBloxWebApiService(webApiServiceUrl);
        });

        private async void ExecuteSearch()
        {
            var searchResults = await _flowBloxWebApiService.Value.GetExtensionsAsync(SearchText);
            if (searchResults != null)
            {
                SearchResults = searchResults;
                OnPropertyChanged(nameof(SearchResults));
            }
        }

        private void ExecuteLogin()
        {
            var loginWindow = new LoginWindow
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (loginWindow.ShowDialog() == true)
            {
                if (loginWindow.Tag != null)
                {
                    var (token, userData) = ((string, FbUserData))loginWindow.Tag;
                    ActiveUser = userData;
                    UserToken = token;
                }
                else
                {
                    ActiveUser = null;
                    UserToken = null;
                }
               
            }
        }

        private bool CanReloadExtensions() => _hasExtensionChanged;

        private async void ReloadExtensions()
        {
            try
            {
                var reloadResult = _project.ReloadExtensions();
                if (!reloadResult.Success)
                {
                    if (reloadResult.RemainingAssemblies.Any())
                    {
                        var projectPath = FlowBloxProjectManager.Instance.ActiveProjectPath;

                        if (!string.IsNullOrEmpty(projectPath))
                        {
                            var errorMessage = string.Format(
                                FlowBloxResourceUtil.GetLocalizedString("Message_RemainingAssemblies_RestartPrompt", typeof(Resources.ExtensionsWindow)),
                                string.Join(", ", reloadResult.RemainingAssemblies));

                            bool? restartConfirmed = await MessageBoxHelper.ShowQuestionAsync(
                                (MetroWindow)_ownerWindow,
                                errorMessage);

                            if (restartConfirmed == true)
                            {
                                try
                                {
                                    
                                    FlowBloxProjectManager.Instance.ActiveProject.Save(projectPath);
                                }
                                catch (Exception ex)
                                {
                                    var projectSaveFailedErrorMessage = string.Format(
                                        FlowBloxResourceUtil.GetLocalizedString("Message_ProjectSaveFailed", typeof(Resources.ExtensionsWindow)),
                                        projectPath, ex.Message);

                                    await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, projectSaveFailedErrorMessage);
                                    return;
                                }

                                string exePath = Environment.ProcessPath;
                                string args = $"--project=\"{projectPath}\"";

                                using var _ = Process.Start(exePath, args);
                                Environment.Exit(0);
                                return;
                            }
                        }
                        else
                        {
                            var fallbackMessage = string.Format(
                                FlowBloxResourceUtil.GetLocalizedString("Message_RemainingAssemblies", typeof(Resources.ExtensionsWindow)),
                                string.Join(", ", reloadResult.RemainingAssemblies));

                            await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, fallbackMessage);
                        }
                    }

                    if (reloadResult.UnloadableExtensions.Any())
                    {
                        var errorMessage = string.Format(FlowBloxResourceUtil.GetLocalizedString("Message_UnloadableExtensions", typeof(Resources.ExtensionsWindow)), string.Join(", ", reloadResult.UnloadableExtensions));
                        await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, errorMessage);
                    }
                }
                else
                {
                    this._hasExtensionChanged = false;
                    this.ReloadExtensionsCommand.Invalidate();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = FlowBloxResourceUtil.GetLocalizedString("Message_ReloadExtensions_Failed", typeof(Resources.ExtensionsWindow));
                await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, errorMessage);
                FlowBloxLogManager.Instance.GetLogger().Error(errorMessage, ex);
            }
        }

        private void LoadExtensions(FlowBloxProject project)
        {
            foreach (var ext in project.Extensions)
            {
                if (!ext.Online)
                    OfflineExtensions.Add(ext);

                InstalledExtensions.Add(ext);

                ext.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(FlowBloxProjectExtension.OfflineExtensionPath))
                    {
                        _hasExtensionChanged = true;
                        CommandManager.InvalidateRequerySuggested();
                    }
                };
            }

            OfflineExtensions.CollectionChanged += (s, e) => UpdateProjectExtensions(project);
            InstalledExtensions.CollectionChanged += (s, e) => UpdateProjectExtensions(project);
        }

        private void UpdateProjectExtensions(FlowBloxProject project)
        {
            project.Extensions = InstalledExtensions.ToList();
        }

        private void AddOfflineExtension()
        {
            var newExtension = new FlowBloxProjectExtension();
            OfflineExtensions.Add(newExtension);
            InstalledExtensions.Add(newExtension);
        }

        private void RemoveOfflineExtension(object parameter)
        {
            if (parameter is FlowBloxProjectExtension extension)
            {
                OfflineExtensions.Remove(extension);
                InstalledExtensions.Remove(extension);
            }
        }

        private void SelectFilePath(object parameter)
        {
            if (parameter is FlowBloxProjectExtension extension)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "DLL files (*.dll)|*.dll",
                    Title = "Select Extension DLL"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    extension.OfflineExtensionPath = openFileDialog.FileName;
                    extension.Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(openFileDialog.FileName);
                    extension.Version = versionInfo.ProductVersion;
                    OnPropertyChanged(nameof(OfflineExtensions));
                }
            }
        }

        private async Task<bool> ValidateExtensionDeletion()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var logger = FlowBloxLogManager.Instance.GetLogger();
            var validator = new ExtensionDeleteValidator(registry, logger);
            var result = validator.Validate(SelectedExtension.Name);

            if (result != ValidationResult.Success)
            {
                await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, result.ErrorMessage);
                return false;
            }

            return true;
        }

        private bool CanUninstallExtension(object parameter) => parameter is FlowBloxProjectExtension;

        private async void UninstallExtension(object parameter)
        {
            if (parameter is FlowBloxProjectExtension extension)
            {
                if (await ValidateExtensionDeletion())
                {
                    InstalledExtensions.Remove(extension);

                    if (!extension.Online)
                        OfflineExtensions.Remove(extension);
                }
            }
        }

        private bool CanAddExtension(object parameter)
        {
            return SelectedVersion != null && !InstalledExtensions.Any(e =>
                e.ExtensionGuid == SelectedExtension.Guid && e.Version == SelectedVersion.Version);
        }

        private ExtensionRepository _extensionRepository;
        private ExtensionCompatibilityValidator _compatibilityValidator;
        private void CreateExtensionRepositoryAsync()
        {
            _extensionRepository = new ExtensionRepository(_flowBloxWebApiService.Value);
            if (SelectedExtension != null)
                _extensionRepository.AddExtension(SelectedExtension);

            _compatibilityValidator = new ExtensionCompatibilityValidator(_extensionRepository);
        }

        bool IsUiAssembly(string assemblyPath)
        {
            try
            {
                var asmName = AssemblyName.GetAssemblyName(assemblyPath);
                var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                var referencedAssemblies = assembly.GetReferencedAssemblies();

                return referencedAssemblies.Any(a =>
                    a.Name == "PresentationFramework" ||
                    a.Name == "PresentationCore" ||
                    a.Name == "WindowsBase" ||
                    a.Name.StartsWith("System.Windows"));
            }
            catch
            {
                return false;
            }
        }


        private async void AddExtension(object parameter)
        {
            if (SelectedVersion != null)
            {
                try
                {
                    CreateExtensionRepositoryAsync();

                    // Validierung der Kompatibilität
                    var validationResult = await _compatibilityValidator.ValidateAsync(SelectedExtension, SelectedVersion, InstalledExtensions);
                    if (validationResult != ValidationResult.Success)
                    {
                        // Fehleranzeige mit MessageBoxHelper
                        await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, validationResult.ErrorMessage);
                        return;
                    }

                    // Mappe die Dependencies von SelectedVersion auf FlowBloxProjectExtensionDependency
                    var dependencies = SelectedVersion.Dependencies
                        .Select(dep => new FlowBloxProjectExtensionDependency(
                            extensionName: dep.ExtensionName,
                            extensionGuid: dep.ExtensionGuid,
                            version: dep.Version))
                        .ToList();

                    var newExtension = new FlowBloxProjectExtension
                    {
                        ExtensionGuid = SelectedExtension.Guid,
                        Name = SelectedExtension.Name,
                        Version = SelectedVersion.Version,
                        Dependencies = dependencies
                    };

                    InstalledExtensions.Add(newExtension);
                    _hasExtensionChanged = true;
                    CommandManager.InvalidateRequerySuggested();
                }
                catch (Exception ex)
                {
                    string errorMessage = FlowBloxResourceUtil.GetLocalizedString("Message_AddExtension_Failed", typeof(Resources.ExtensionsWindow));
                    await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, errorMessage);
                    FlowBloxLogManager.Instance.GetLogger().Error(errorMessage, ex);
                }
            }
        }

        private bool CanRemoveExtension(object parameter)
        {
            return SelectedVersion != null && InstalledExtensions.Any(e =>
                e.ExtensionGuid == SelectedExtension.Guid && e.Version == SelectedVersion.Version);
        }

        private async void RemoveExtension(object parameter)
        {
            var extensionToRemove = InstalledExtensions.FirstOrDefault(e =>
                e.ExtensionGuid == SelectedExtension.Guid && e.Version == SelectedVersion.Version);

            if (extensionToRemove != null)
            {
                if (await ValidateExtensionDeletion())
                {
                    InstalledExtensions.Remove(extensionToRemove);
                    _hasExtensionChanged = true;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private bool CanModifyExtension(object parameter) => parameter is FlowBloxProjectExtension;

        private bool CanUpdateExtension(object parameter)
        {
            return SelectedVersion != null && InstalledExtensions.Any(e =>
                e.ExtensionGuid == SelectedExtension.Guid && e.Version != SelectedVersion.Version);
        }

        private async void UpdateExtension(object parameter)
        {
            if (SelectedVersion != null)
            {
                try
                {
                    CreateExtensionRepositoryAsync();

                    // Validierung der Kompatibilität
                    var validationResult = await _compatibilityValidator.ValidateAsync(SelectedExtension, SelectedVersion, InstalledExtensions);
                    if (validationResult != ValidationResult.Success)
                    {
                        // Fehleranzeige mit MessageBoxHelper
                        await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, validationResult.ErrorMessage);
                        return;
                    }

                    var extensionToUpdate = InstalledExtensions.FirstOrDefault(e =>
                        e.ExtensionGuid == SelectedExtension.Guid);

                    if (extensionToUpdate != null)
                    {
                        extensionToUpdate.Version = SelectedVersion.Version;
                        extensionToUpdate.Dependencies = SelectedVersion.Dependencies
                            .Select(dep => new FlowBloxProjectExtensionDependency(
                                extensionName: dep.ExtensionName,
                                extensionGuid: dep.ExtensionGuid,
                                version: dep.Version))
                            .ToList();

                        _hasExtensionChanged = true;
                        CommandManager.InvalidateRequerySuggested();
                    }
                }
                catch(Exception ex)
                {
                    string errorMessage = FlowBloxResourceUtil.GetLocalizedString("Message_UpdateExtension_Failed", typeof(Resources.ExtensionsWindow));
                    await MessageBoxHelper.ShowMessageBoxAsync((MetroWindow)_ownerWindow, MessageBoxType.Error, errorMessage);
                    FlowBloxLogManager.Instance.GetLogger().Error(errorMessage, ex);
                }
            }
        }
    }
}
