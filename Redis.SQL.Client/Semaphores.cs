using System.Collections.Generic;
using System.Threading;

namespace Redis.SQL.Client
{
    internal class Semaphores
    {
        private static readonly IDictionary<string, SemaphoreSlim> EntityLocks = new Dictionary<string, SemaphoreSlim>();

        private static readonly IDictionary<string, SemaphoreSlim> IndexLocks = new Dictionary<string, SemaphoreSlim>();

        private static readonly object EntityDictionaryLock = new object();

        private static readonly object IndexDictionaryLock = new object();

        internal static SemaphoreSlim GetEntitySemaphore(string entityName)
        {
            lock (EntityDictionaryLock)
            {
                entityName = entityName.Trim().ToLower();
                if (EntityLocks.TryGetValue(entityName, out var mutex)) return mutex;
                mutex = new SemaphoreSlim(1, 1);
                EntityLocks.Add(entityName, mutex);
                return mutex;
            }
        }

        internal static SemaphoreSlim GetIndexSemaphore(string hashSet, string key)
        {
            lock (IndexDictionaryLock)
            {
                var lookupKey = $"{hashSet.Trim().ToLower()}:{key.Trim().ToLower()}";
                if (IndexLocks.TryGetValue(lookupKey, out var mutex)) return mutex;
                mutex = new SemaphoreSlim(1, 1);
                IndexLocks.Add(lookupKey, mutex);
                return mutex;
            }
        }
    }
}