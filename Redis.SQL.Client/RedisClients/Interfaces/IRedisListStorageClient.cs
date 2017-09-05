using System.Threading.Tasks;

namespace Redis.SQL.Client.RedisClients.Interfaces
{
    internal interface IRedisListStorageClient
    {
        Task<string> GetListElementByIndex(string key, int index);
        Task<long> AddToListTail<T>(string key, T value);
        Task<long> AddToListHead<T>(string key, T value);
    }
}
