using System.Threading.Tasks;

namespace Redis.SQL.Client.Interfaces
{
    internal interface IRedisSetStorageClient
    {
        Task<bool> SetContains<T>(string key, T value);
        Task<bool> AddToSet<T>(string key, T value);
    }
}
