using FlowBlox.Core.Util;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using FlowBlox.Core.Logging;
using FlowBlox.Core;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.UICore.Interfaces;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.UICore.Enums;
using SkiaSharp;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Grid.Elements.UI.CustomActions
{
    public class FileObjectUIActions : ComponentUIActions<FileObject>
    {
        private readonly IFlowBloxMessageBoxService _messageBoxService;

        public FileObjectUIActions(FileObject component) : base(component)
        {
            _messageBoxService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>();
        }

        public SKImage OpenIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_edit, 16, SKColors.DodgerBlue);

        public bool CanOpen()
        {
            return Component.CanRead();
        }

        [Display(Name = "FileObjectUIActions_Open", ResourceType = typeof(FlowBloxTexts))]
        public void Open()
        {
            if (!Component.CanRead())
            {
                _messageBoxService.ShowMessageBox(
                    "The file is not ready for opening.",
                    "Cannot Open File",
                    FlowBloxMessageBoxTypes.Information
                );
                return;
            }

            string filePath = Component.GetRuntimeFilePath();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _messageBoxService.ShowMessageBox(
                   "The file does not exist or cannot be accessed. Path: " + filePath,
                   "File does not exist",
                   FlowBloxMessageBoxTypes.Information
               );
                return;
            }

            try
            {
                Process.Start("explorer.exe", $"\"{filePath}\"");
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);

                _messageBoxService.ShowMessageBox(
                   "The file could not be opened due to technical problems. Path: " + filePath,
                   "File could not be opened",
                   FlowBloxMessageBoxTypes.Error
               );
            }
        }

        public SKImage DeleteIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_document_remove, 16, SKColors.IndianRed);

        public bool CanDelete()
        {
            string filePath = Component.GetRuntimeFilePath();
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }

        [Display(Name = "FileObjectUIActions_Delete", ResourceType = typeof(FlowBloxTexts))]
        public void Delete()
        {
            string filePath = Component.GetRuntimeFilePath();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _messageBoxService.ShowMessageBox(
                   "The file does not exist or cannot be accessed. Path: " + filePath,
                   "File does not exist",
                   FlowBloxMessageBoxTypes.Information
               );
                return;
            }

            try
            {
                File.Delete(filePath);
                _messageBoxService.ShowMessageBox(
                    "The file has been successfully deleted.",
                    "File Deleted",
                    FlowBloxMessageBoxTypes.Information
                );
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);

                _messageBoxService.ShowMessageBox(
                    "The file could not be deleted due to technical problems. Path: " + filePath,
                    "File could not be deleted",
                    FlowBloxMessageBoxTypes.Error
                );
            }
        }

        public SKImage OpenDirectoryIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.folder_open, 16, SKColors.DarkGoldenrod);

        public bool CanOpenDirectory()
        {
            string filePath = Component.GetRuntimeFilePath();
            if (string.IsNullOrEmpty(filePath))
                return false;

            string directory = Path.GetDirectoryName(filePath);
            return Directory.Exists(directory);
        }

        [Display(Name = "FileObjectUIActions_OpenDirectory", ResourceType = typeof(FlowBloxTexts))]
        public void OpenDirectory()
        {
            string filePath = Component.GetRuntimeFilePath();
            if (string.IsNullOrEmpty(filePath))
                return;

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                _messageBoxService.ShowMessageBox(
                    "The directory does not exist. Path: " + directory,
                    "Directory does not exist",
                    FlowBloxMessageBoxTypes.Information
                );
                return;
            }

            try
            {
                Process.Start("explorer.exe", $"\"{directory}\"");
            }
            catch (Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);

                _messageBoxService.ShowMessageBox(
                    "The directory could not be opened due to technical problems. Path: " + directory,
                    "Directory could not be opened",
                    FlowBloxMessageBoxTypes.Error
                  );
               
            }
        }
    }
}
