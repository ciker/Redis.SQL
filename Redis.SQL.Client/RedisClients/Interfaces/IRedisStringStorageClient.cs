using System.Threading.Tasks;

namespace Redis.SQL.Client.RedisClients.Interfaces
{
    internal interface IRedisStringStorageClient
    {
        Task<string> GetValue(string key);
        Task<bool> StoreValue<T>(string key, T value);
        Task<long> IncrementValue(string key);
        Task<long> DecrementValue(string key);
        Task<bool> DeleteValue(string key);
    }
}
