using System.Reflection;

namespace FlowBlox.SequenceDetection.Util
{
    public class ResourceUtil
    {
        public static string GetResourceAsStringRelatedTo<T>(Assembly assembly, string relativeResourceName)
        {
            string resourceName = typeof(T).Namespace + "." + relativeResourceName;
            return GetResourceAsString(assembly, resourceName);
        }

        public static string GetResourceAsString(Assembly assembly, string resourceName)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (resourceName == null)
                throw new ArgumentNullException(nameof(resourceName));
                
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException($"Resource \"{resourceName}\" not found");

            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static byte[] GetResourceAsByteArray(Assembly assembly, string resourceName)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (resourceName == null)
                throw new ArgumentNullException(nameof(resourceName));

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException($"Resource \"{resourceName}\" not found");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}