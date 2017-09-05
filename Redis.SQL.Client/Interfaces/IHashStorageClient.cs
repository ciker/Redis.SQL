using System.Threading.Tasks;

namespace Redis.SQL.Client.Interfaces
{
    internal interface IRedisHashStorageClient
    {
        Task<string> GetHashField(string hashSet, string key);
        Task<bool> StoreHashField<T>(string hashSet, string key, T value);
    }
}
