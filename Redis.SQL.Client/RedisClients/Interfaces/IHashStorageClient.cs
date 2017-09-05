using System.Threading.Tasks;

namespace Redis.SQL.Client.RedisClients.Interfaces
{
    internal interface IRedisHashStorageClient
    {
        Task<string> GetHashField(string hashSet, string key);
        Task<bool> SetHashField<T>(string hashSet, string key, T value);
        Task<bool> AppendStringToHashField(string hashSet, string key, string value);
        Task<bool> RemoveStringFromHashField(string hashSet, string key, string value);
    }
}
