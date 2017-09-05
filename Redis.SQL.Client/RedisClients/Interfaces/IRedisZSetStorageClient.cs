using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SQL.Client.RedisClients.Interfaces
{
    internal interface IRedisZSetStorageClient
    {
        Task<IEnumerable<string>> GetSortedSetElementsByScore(string key, double minScore, double maxScore);
        Task<IEnumerable<string>> GetSortedSetElementsByIndex(string key, long startIndex, long endIndex);
        Task<IEnumerable<string>> GetSortedSetElementsByValue(string key, string minValue, string maxValue);
        Task<bool> AddToSortedSet<T>(string key, T value, double score = 0D);
    }
}
