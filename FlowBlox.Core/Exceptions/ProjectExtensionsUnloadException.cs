using System.Text;

namespace FlowBlox.Core.Exceptions
{
    public class ProjectExtensionsUnloadException : Exception
    {
        public IReadOnlyList<string> RemainingAssemblies { get; }
        public IReadOnlyList<string> UnloadableExtensions { get; }

        public ProjectExtensionsUnloadException(List<string> remainingAssemblies, List<string> unloadableExtensions)
            : base(GenerateMessage(remainingAssemblies, unloadableExtensions))
        {
            RemainingAssemblies = remainingAssemblies;
            UnloadableExtensions = unloadableExtensions;
        }

        private static string GenerateMessage(List<string> remainingAssemblies, List<string> unloadableExtensions)
        {
            var messageBuilder = new StringBuilder("Project extensions could not be fully unloaded.");

            if (remainingAssemblies?.Any() == true)
            {
                messageBuilder.AppendLine()
                             .AppendLine("Remaining assemblies:")
                             .AppendLine(string.Join(", ", remainingAssemblies));
            }

            if (unloadableExtensions?.Any() == true)
            {
                messageBuilder.AppendLine()
                             .AppendLine("Unloadable extensions:")
                             .AppendLine(string.Join(", ", unloadableExtensions));
            }

            return messageBuilder.ToString();
        }
    }
}
