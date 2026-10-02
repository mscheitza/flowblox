using FlowBlox.Core.Models.Runtime;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    internal sealed class FlowBloxRuntimeModelCache<TSession>
        where TSession : class, IDisposable
    {
        private readonly ConcurrentDictionary<CacheKey, TSession> _sessions =
            new(new CacheKeyComparer());

        public TSession Open(
            BaseRuntime runtime,
            string modelFolder,
            Func<TSession> sessionFactory,
            out bool alreadyOpen)
        {
            ArgumentNullException.ThrowIfNull(runtime);
            ArgumentNullException.ThrowIfNull(sessionFactory);

            var key = new CacheKey(runtime, NormalizeModelFolder(modelFolder));
            if (_sessions.TryGetValue(key, out var existing))
            {
                alreadyOpen = true;
                return existing;
            }

            var created = sessionFactory()
                ?? throw new InvalidOperationException("The model session factory returned null.");
            var cached = _sessions.GetOrAdd(key, created);
            alreadyOpen = !ReferenceEquals(created, cached);
            if (alreadyOpen)
                created.Dispose();
            return cached;
        }

        public bool Close(BaseRuntime runtime, string modelFolder)
        {
            var key = new CacheKey(runtime, NormalizeModelFolder(modelFolder));
            if (!_sessions.TryRemove(key, out var session))
                return false;

            session.Dispose();
            return true;
        }

        private static string NormalizeModelFolder(string modelFolder)
        {
            if (string.IsNullOrWhiteSpace(modelFolder))
                throw new ArgumentException("The model folder must not be empty.", nameof(modelFolder));

            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(modelFolder));
        }

        private readonly record struct CacheKey(BaseRuntime Runtime, string ModelFolder);

        private sealed class CacheKeyComparer : IEqualityComparer<CacheKey>
        {
            private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            public bool Equals(CacheKey x, CacheKey y) =>
                ReferenceEquals(x.Runtime, y.Runtime)
                && PathComparer.Equals(x.ModelFolder, y.ModelFolder);

            public int GetHashCode(CacheKey key) => HashCode.Combine(
                RuntimeHelpers.GetHashCode(key.Runtime),
                PathComparer.GetHashCode(key.ModelFolder));
        }
    }
}
