using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SQL.Client.RedisClients.Interfaces
{
    internal interface IRedisSetStorageClient
    {
        Task<bool> SetContains<T>(string key, T value);
        Task<bool> AddToSet<T>(string key, T value);
        Task<IEnumerable<string>> GetSetMembers(string key);
        Task<bool> RemoveFromSetByValue(string key, string value);
    }
}
