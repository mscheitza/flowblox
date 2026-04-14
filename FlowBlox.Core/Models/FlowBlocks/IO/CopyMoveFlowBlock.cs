using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.IO
{
    [Display(Name = "CopyMoveFlowBlock_DisplayName", Description = "CopyMoveFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class CopyMoveFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "CopyMoveFlowBlock_Mode", Description = "CopyMoveFlowBlock_Mode_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public CopyMoveMode Mode { get; set; } 

        [ActivationCondition(MemberName = nameof(Mode), Values = new object[] { CopyMoveMode.CopyFile, CopyMoveMode.MoveFile })]
        [ConditionallyRequired]
        [Display(Name = "CopyMoveFlowBlock_SourceFile", Description = "CopyMoveFlowBlock_SourceFile_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFileSelection | UIOptions.EnableFieldSelection)]
        public string SourceFile { get; set; }

        [ActivationCondition(MemberName = nameof(Mode), Values = new object[] { CopyMoveMode.CopyDirectory, CopyMoveMode.MoveDirectory })]
        [ConditionallyRequired]
        [Display(Name = "CopyMoveFlowBlock_SourceFolder", Description = "CopyMoveFlowBlock_SourceFolder_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFolderSelection | UIOptions.EnableFieldSelection)]
        public string SourceFolder { get; set; }

        [Display(Name = "CopyMoveFlowBlock_DestinationPath", Description = "CopyMoveFlowBlock_DestinationPath_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFileSelection | UIOptions.EnableFolderSelection | UIOptions.EnableFieldSelection)]
        [Required]
        public string DestinationPath { get; set; }

        [ActivationCondition(MemberName = nameof(Mode), Values = new object[] { CopyMoveMode.CopyDirectory })]
        [Display(Name = "CopyMoveFlowBlock_Recursive", Description = "CopyMoveFlowBlock_Recursive_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        public bool Recursive { get; set; }

        [ActivationCondition(MemberName = nameof(Mode), Values = new object[] { CopyMoveMode.CopyFile, CopyMoveMode.MoveFile })]
        [Display(Name = "CopyMoveFlowBlock_FileOverwrite", Description = "CopyMoveFlowBlock_FileOverwrite_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        public bool FileOverwrite { get; set; }

        [ActivationCondition(MemberName = nameof(Mode), Values = new object[] { CopyMoveMode.CopyDirectory, CopyMoveMode.MoveDirectory })]
        [Display(Name = "CopyMoveFlowBlock_DirectoryOverwrite", Description = "CopyMoveFlowBlock_DirectoryOverwrite_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        public bool DirectoryOverwrite { get; set; }

        public CopyMoveFlowBlock()
        {
            Mode = CopyMoveMode.CopyFile;
            Recursive = true;
            FileOverwrite = true;
            DirectoryOverwrite = true;
        }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_export, 16, SKColors.SteelBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.file_export, 32, SKColors.SteelBlue);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.IO;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(Mode));
            properties.Add(nameof(SourceFile));
            properties.Add(nameof(SourceFolder));
            properties.Add(nameof(DestinationPath));
            properties.Add(nameof(Recursive));
            properties.Add(nameof(FileOverwrite));
            properties.Add(nameof(DirectoryOverwrite));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var resolvedDestination = ResolveRequiredPath(DestinationPath, nameof(DestinationPath));
                string finalDestination;

                switch (Mode)
                {
                    case CopyMoveMode.CopyFile:
                    {
                        var sourceFile = ResolveRequiredPath(SourceFile, nameof(SourceFile));
                        finalDestination = CopyFile(sourceFile, resolvedDestination, FileOverwrite);
                        break;
                    }
                    case CopyMoveMode.MoveFile:
                    {
                        var sourceFile = ResolveRequiredPath(SourceFile, nameof(SourceFile));
                        finalDestination = MoveFile(sourceFile, resolvedDestination, FileOverwrite);
                        break;
                    }
                    case CopyMoveMode.CopyDirectory:
                    {
                        var sourceDirectory = ResolveRequiredPath(SourceFolder, nameof(SourceFolder));
                        finalDestination = CopyDirectory(sourceDirectory, resolvedDestination, Recursive, FileOverwrite, DirectoryOverwrite);
                        break;
                    }
                    case CopyMoveMode.MoveDirectory:
                    {
                        var sourceDirectory = ResolveRequiredPath(SourceFolder, nameof(SourceFolder));
                        finalDestination = MoveDirectory(sourceDirectory, resolvedDestination, FileOverwrite, DirectoryOverwrite);
                        break;
                    }
                    default:
                        throw new NotSupportedException($"Unsupported mode: {Mode}");
                }

                GenerateResult(runtime, finalDestination);
            });
        }

        private static string ResolveRequiredPath(string value, string propertyName)
        {
            var resolved = FlowBloxFieldHelper.ReplaceFieldsInString(value ?? string.Empty);
            if (string.IsNullOrWhiteSpace(resolved))
                throw new InvalidOperationException($"No path was provided for '{propertyName}'.");

            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(resolved).Trim());
        }

        private static string CopyFile(string sourceFile, string destinationPath, bool overwrite)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException("Source file was not found.", sourceFile);

            var destinationFile = ResolveDestinationFilePath(sourceFile, destinationPath);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (string.IsNullOrWhiteSpace(destinationDirectory))
                throw new InvalidOperationException("No destination directory could be resolved for file copy.");

            Directory.CreateDirectory(destinationDirectory);
            File.Copy(sourceFile, destinationFile, overwrite);
            return destinationFile;
        }

        private static string MoveFile(string sourceFile, string destinationPath, bool overwrite)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException("Source file was not found.", sourceFile);

            var destinationFile = ResolveDestinationFilePath(sourceFile, destinationPath);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (string.IsNullOrWhiteSpace(destinationDirectory))
                throw new InvalidOperationException("No destination directory could be resolved for file move.");

            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            File.Move(sourceFile, destinationFile, overwrite);
            return destinationFile;
        }

        private static string ResolveDestinationFilePath(string sourceFile, string destinationPath)
        {
            if (Directory.Exists(destinationPath))
                return Path.Combine(destinationPath, Path.GetFileName(sourceFile));

            if (destinationPath.EndsWith(Path.DirectorySeparatorChar) || 
                destinationPath.EndsWith(Path.AltDirectorySeparatorChar))
            {
                return Path.Combine(destinationPath, Path.GetFileName(sourceFile));
            }

            return destinationPath;
        }

        private static string CopyDirectory(string sourceDirectory, string destinationDirectory, bool recursive, bool fileOverwrite, bool directoryOverwrite)
        {
            if (!Directory.Exists(sourceDirectory))
                throw new DirectoryNotFoundException($"Source directory \"{sourceDirectory}\" was not found.");

            if (Directory.Exists(destinationDirectory) && !directoryOverwrite)
                throw new IOException($"Destination directory \"{destinationDirectory}\" already exists.");

            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            foreach (var sourceFilePath in Directory.EnumerateFiles(sourceDirectory))
            {
                var fileName = Path.GetFileName(sourceFilePath);
                var targetFilePath = Path.Combine(destinationDirectory, fileName);
                File.Copy(sourceFilePath, targetFilePath, fileOverwrite);
            }

            if (recursive)
            {
                foreach (var sourceSubDirectory in Directory.EnumerateDirectories(sourceDirectory))
                {
                    var subDirectoryName = Path.GetFileName(sourceSubDirectory);
                    var targetSubDirectory = Path.Combine(destinationDirectory, subDirectoryName);
                    CopyDirectory(sourceSubDirectory, targetSubDirectory, recursive: true, fileOverwrite: fileOverwrite, directoryOverwrite: directoryOverwrite);
                }
            }

            return destinationDirectory;
        }

        private static string MoveDirectory(string sourceDirectory, string destinationDirectory, bool fileOverwrite, bool directoryOverwrite)
        {
            if (!Directory.Exists(sourceDirectory))
                throw new DirectoryNotFoundException($"Source directory \"{sourceDirectory}\" was not found.");

            if (Path.GetFullPath(sourceDirectory).Equals(Path.GetFullPath(destinationDirectory), StringComparison.OrdinalIgnoreCase))
                return destinationDirectory;

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.Move(sourceDirectory, destinationDirectory);
                return destinationDirectory;
            }

            if (!directoryOverwrite)
                throw new IOException($"Destination directory \"{destinationDirectory}\" already exists.");

            foreach (var sourceFilePath in Directory.EnumerateFiles(sourceDirectory))
            {
                var fileName = Path.GetFileName(sourceFilePath);
                var targetFilePath = Path.Combine(destinationDirectory, fileName);
                File.Move(sourceFilePath, targetFilePath, overwrite: fileOverwrite);
            }

            foreach (var sourceSubDirectory in Directory.EnumerateDirectories(sourceDirectory))
            {
                var subDirectoryName = Path.GetFileName(sourceSubDirectory);
                var targetSubDirectory = Path.Combine(destinationDirectory, subDirectoryName);
                MoveDirectory(sourceSubDirectory, targetSubDirectory, fileOverwrite: fileOverwrite, directoryOverwrite: directoryOverwrite);
            }

            if (!Directory.EnumerateFileSystemEntries(sourceDirectory).Any())
                Directory.Delete(sourceDirectory, recursive: false);

            return destinationDirectory;
        }
    }
}
