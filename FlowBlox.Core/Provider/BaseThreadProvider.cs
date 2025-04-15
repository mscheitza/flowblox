using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FlowBlox.Core.Provider
{
    public abstract class BaseThreadProvider<T> : IDisposable
    {
        private int threadId;

        protected static readonly object MapLock = new object();

        private class ThreadEntry
        {
            public Type Type { get; set; }

            public int ThreadId { get; set; }

            public T ManagedObject { get; set; }
        }

        protected static T GetManagedObject(Type type)
        {
            lock (MapLock)
            {
                var threadEntry = _threadEntries.FirstOrDefault(x => x.ThreadId == Thread.CurrentThread.ManagedThreadId && x.Type == type);
                if (threadEntry != null)
                    return threadEntry.ManagedObject;
                return default(T);
            }
        }

        private static readonly HashSet<ThreadEntry> _threadEntries = new HashSet<ThreadEntry>();

        private void Init(int threadId, T managedObject)
        {
            lock (MapLock)
            {
                this.threadId = threadId;

                if (!_threadEntries.Any(x => x.ThreadId == threadId && x.Type == GetType()))
                    _threadEntries.Add(new ThreadEntry() { Type = GetType(), ManagedObject = managedObject, ThreadId = threadId });
            }
        }

        protected BaseThreadProvider(T managedObject)
        {
            Init(Thread.CurrentThread.ManagedThreadId, managedObject);
        }

        public void Dispose()
        {
            lock (MapLock)
            {
                _threadEntries.RemoveWhere(x => x.ThreadId == threadId && x.Type == GetType());
            }
        }
    }
}
