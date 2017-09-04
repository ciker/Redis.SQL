using System.Threading.Tasks;

namespace Redis.SQL.Client.Interfaces
{
    internal interface IStorageClient
    {
        Task<T> GetValue<T>(string key);
        Task<bool> StoreValue<T>(string key, T value);
        Task<T> GetHashField<T>(string hashSet, string key);
        Task<bool> StoreHashField<T>(string hashSet, string key, T value);
        Task<bool> SetContains<T>(string key, T value);
        Task<bool> AddToSet<T>(string key, T value);
    }
}
