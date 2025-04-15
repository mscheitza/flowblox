using FlowBlox.Core.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace FlowBlox.Core.Util
{
    public static class AssemblyVerifier
    {
        public static bool VerifyAssembly(string assemblyPath)
        {
            try
            {
                // Load the executing assembly's public key token
                var executingAssembly = Assembly.GetExecutingAssembly();
                var executingPublicKeyTokenBytes = executingAssembly.GetName().GetPublicKeyToken();
                var executingPublicKeyTokenString = BitConverter.ToString(executingPublicKeyTokenBytes).Replace("-", string.Empty).ToUpperInvariant();

                // Find the assembly already loaded in the AppDomain by its path
                var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.Location, assemblyPath, StringComparison.OrdinalIgnoreCase));

                if (loadedAssembly == null)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error($"Assembly not found in loaded assemblies: {assemblyPath}");
                    return false;
                }

                // Get the public key token of the loaded assembly
                var assemblyPublicKeyTokenBytes = loadedAssembly.GetName().GetPublicKeyToken();
                var assemblyPublicKeyTokenString = BitConverter.ToString(assemblyPublicKeyTokenBytes).Replace("-", string.Empty).ToUpperInvariant();

                // Compare the public key tokens
                if (assemblyPublicKeyTokenString == executingPublicKeyTokenString)
                {
                    FlowBloxLogManager.Instance.GetLogger().Info($"Assembly {loadedAssembly.FullName} is valid.");
                    return true;
                }
                else
                {
                    FlowBloxLogManager.Instance.GetLogger().Error($"Invalid public key token: {assemblyPublicKeyTokenString}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error($"Error while verifying the assembly: {ex.Message}");
                return false;
            }
        }
    }
}
