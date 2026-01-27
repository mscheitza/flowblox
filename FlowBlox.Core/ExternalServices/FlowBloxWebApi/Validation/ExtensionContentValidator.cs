using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Validation
{
    public class ExtensionContentValidator
    {
        public async Task<ValidationResult> ValidateAsync(byte[] content, string extensionName, string expectedVersion)
        {
            try
            {
                using (var memoryStream = new MemoryStream(content))
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var dllEntries = archive.Entries.Where(entry => entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToList();
                    var depsJsonEntry = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase));

                    // Check if there is at least one DLL file
                    if (!dllEntries.Any())
                    {
                        return new ValidationResult("The ZIP archive does not contain any DLL files.");
                    }

                    // Check if the main DLL is present
                    string mainAssemblyFullName = $"{extensionName}.dll";
                    var mainDllEntry = dllEntries.FirstOrDefault(entry => entry.Name.Equals(mainAssemblyFullName, StringComparison.OrdinalIgnoreCase));
                    if (mainDllEntry == null)
                    {
                        return new ValidationResult($"The ZIP archive does not contain the main assembly '{mainAssemblyFullName}'.");
                    }

                    // Check if the .deps.json file exists
                    if (depsJsonEntry == null)
                    {
                        return new ValidationResult("The ZIP archive does not contain a .deps.json file.");
                    }

                    var tempDllPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                    try
                    {
                        using (var dllStream = mainDllEntry.Open())
                        using (var fileStream = new FileStream(tempDllPath, FileMode.Create, FileAccess.Write))
                        {
                            await dllStream.CopyToAsync(fileStream);
                        }

                        // Retrieve and compare the product version of the extracted DLL
                        var fileVersionInfo = FileVersionInfo.GetVersionInfo(tempDllPath);
                        var productVersion = Regex.Replace(fileVersionInfo.ProductVersion, @"\+.*$", "");
                        if (!productVersion.Equals(expectedVersion, StringComparison.OrdinalIgnoreCase))
                        {
                            return new ValidationResult($"The main assembly product version '{productVersion}' does not match the expected version '{expectedVersion}'.");
                        }
                    }
                    finally
                    {
                        if (File.Exists(tempDllPath))
                            File.Delete(tempDllPath);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult($"Error processing ZIP archive: {ex.Message}");
            }

            return ValidationResult.Success;
        }
    }
}
