using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using SkiaSharp;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace FlowBlox.Grid.Elements.UI.CustomActions
{
    public class MemoryObjectUIActions : ComponentUIActions<MemoryObject>
    {
        private static readonly ConcurrentDictionary<MemoryObject, string> _openFilePaths = new();
        private static readonly ConcurrentDictionary<MemoryObject, byte[]> _originalHashes = new();
        private static readonly ConcurrentDictionary<MemoryObject, CancellationTokenSource> _cancellationTokens = new();

        private static readonly TimeSpan MonitoringDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);

        private readonly IFlowBloxMessageBoxService _messageBoxService;
        private readonly IDialogService _dialogService;

        public MemoryObjectUIActions(MemoryObject component) : base(component)
        {
            _messageBoxService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>();
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
        }

        public SKImage OpenIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_edit, 16, SKColors.DodgerBlue);

        public bool CanOpen()
        {
            return Component.CanRead();
        }

        [Display(Name = "MemoryObjectUIActions_Open", ResourceType = typeof(FlowBloxTexts))]
        public void Open()
        {
            try
            {
                Component.CreateTemporaryFile();
            }
            catch (IOException e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);
                _messageBoxService.ShowMessageBox(
                    "The temporary file cannot currently be accessed. Please make sure that all applications that access the file are closed (e.g. Excel or Word).",
                    "File not accessible",
                    FlowBloxMessageBoxTypes.Information);
                return;
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);
                _messageBoxService.ShowMessageBox(
                    "The temporary file could not be created due to technical problems.",
                    "File Creation Failed",
                    FlowBloxMessageBoxTypes.Error);
                return;
            }

            if (!Component.CanReadTemporaryFile())
            {
                _messageBoxService.ShowMessageBox(
                    "The MemoryObject is not ready for opening.",
                    "Cannot Open MemoryObject",
                    FlowBloxMessageBoxTypes.Information);
                return;
            }

            string filePath = Component.TemporaryFilePath;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _messageBoxService.ShowMessageBox(
                    "The file does not exist or could not be created. Path: " + filePath,
                    "File does not exist",
                    FlowBloxMessageBoxTypes.Information);
                return;
            }

            if (_cancellationTokens.TryGetValue(Component, out var token))
                token.Cancel();

            byte[] originalHash = ComputeHash(File.ReadAllBytes(filePath));
            _openFilePaths[Component] = filePath;
            _originalHashes[Component] = originalHash;

            var cts = new CancellationTokenSource();
            _cancellationTokens[Component] = cts;
            _ = MonitorFileChangesAsync(Component, filePath, originalHash, cts.Token);

            try
            {
                var dialog = new MultiValueSelectionDialog(
                    FlowBloxResourceUtil.GetLocalizedString("FileOpenMode_Dialog_Title", typeof(FlowBloxTexts)),
                    FlowBloxResourceUtil.GetLocalizedString("FileOpenMode_Dialog_Message", typeof(FlowBloxTexts)),
                    new GenericSelectionHandler<FileOpenMode>(
                        Enum.GetValues(typeof(FileOpenMode)).Cast<FileOpenMode>().ToList(),
                        mode => mode.GetDisplayName()
                    )
                );

                if (_dialogService.ShowWPFDialog(dialog, true) == true)
                {
                    switch (dialog.SelectedItem.Value)
                    {
                        case FileOpenMode.FlowBloxEditor:
                            FlowBloxEditingHelper.OpenUsingEditor(filePath);
                            break;

                        case FileOpenMode.WindowsDefaultApp:
                            Process.Start("explorer.exe", $"\"{filePath}\"");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);
                _messageBoxService.ShowMessageBox(
                    "The file could not be opened due to technical problems. Path: " + filePath,
                    "File could not be opened",
                    FlowBloxMessageBoxTypes.Error);
            }
        }

        private async Task MonitorFileChangesAsync(MemoryObject memoryObject, string filePath, byte[] originalHash, CancellationToken cancellationToken)
        {
            DateTime start = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested && DateTime.Now - start < MonitoringDuration)
            {
                try
                {
                    if (!File.Exists(filePath))
                        break;

                    byte[] currentBytes;
                    try
                    {
                        currentBytes = File.ReadAllBytes(filePath);
                    }
                    catch (IOException)
                    {
                        await Task.Delay(PollingInterval, cancellationToken);
                        continue;
                    }

                    byte[] currentHash = ComputeHash(File.ReadAllBytes(filePath));
                    if (!originalHash.SequenceEqual(currentHash))
                    {
                        memoryObject.Content = File.ReadAllBytes(filePath);
                        _originalHashes[memoryObject] = currentHash;

                        CleanupMonitoring(memoryObject);

                        if (Application.OpenForms.Count > 0)
                        {
                            var ownerForm = Application.OpenForms[0];
                            ownerForm?.BeginInvoke(new Action(() =>
                            {
                                _messageBoxService.ShowMessageBox(
                                    "The content of the temporary file has changed and was imported into the MemoryObject.",
                                    "File Modified",
                                    FlowBloxMessageBoxTypes.Information);
                            }));
                        }

                        return;
                    }
                }
                catch (Exception ex)
                {
                    FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                    break;
                }

                try
                {
                    await Task.Delay(PollingInterval, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    FlowBloxLogManager.Instance.GetLogger().Info("File monitoring task was cancelled. Polling loop terminated gracefully.");
                    return;
                }
            }

            CleanupMonitoring(memoryObject);
        }

        private void CleanupMonitoring(MemoryObject memoryObject)
        {
            if (_cancellationTokens.TryRemove(memoryObject, out var cts))
                cts.Cancel();

            _openFilePaths.TryRemove(memoryObject, out _);
            _originalHashes.TryRemove(memoryObject, out _);
        }

        private static byte[] ComputeHash(byte[] data)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(data);
        }

        public SKImage ExportIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_export, 16, SKColors.SteelBlue);

        public bool CanExport()
        {
            return Component.CanRead();
        }

        [Display(Name = "MemoryObjectUIActions_Export", ResourceType = typeof(FlowBloxTexts))]
        public void Export()
        {
            using var dialog = new SaveFileDialog
            {
                FileName = Component.FileName,
                Filter = "All Files|*.*",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                File.WriteAllBytes(dialog.FileName, Component.Content);
                _messageBoxService.ShowMessageBox(
                    "The content has been exported successfully.",
                    "Export Successful",
                    FlowBloxMessageBoxTypes.Information);
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);
                _messageBoxService.ShowMessageBox(
                    "The content could not be exported due to technical problems.",
                    "Export Failed",
                    FlowBloxMessageBoxTypes.Error);
            }
        }

        public SKImage ImportIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_import, 16, SKColors.MediumSeaGreen);

        [Display(Name = "MemoryObjectUIActions_Import", ResourceType = typeof(FlowBloxTexts))]
        public void Import()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                Component.Content = File.ReadAllBytes(dialog.FileName);
                Component.FileName = Path.GetFileName(dialog.FileName);

                _messageBoxService.ShowMessageBox(
                    "The file has been successfully imported.",
                    "Import Successful",
                    FlowBloxMessageBoxTypes.Information);
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);
                _messageBoxService.ShowMessageBox(
                    "The file could not be imported due to technical problems.",
                    "Import Failed",
                    FlowBloxMessageBoxTypes.Error);
            }
        }
    }
}
