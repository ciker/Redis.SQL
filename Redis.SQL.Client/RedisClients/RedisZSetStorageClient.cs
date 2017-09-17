using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.RedisClients.Interfaces;
using StackExchange.Redis;

namespace Redis.SQL.Client.RedisClients
{
    internal class RedisZSetStorageClient : IRedisZSetStorageClient
    {
        private readonly IDatabase _redisDatabase;

        internal RedisZSetStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }

        public async Task<IEnumerable<string>> GetSortedSetElementsByScore(string key, double minScore, double maxScore)
        {
            return (await _redisDatabase.SortedSetRangeByScoreAsync(key.ToLower(), minScore, maxScore)).Select(x => x.ToString());
        }

        public async Task<IEnumerable<string>> GetSortedSetElementsByIndex(string key, long startIndex, long endIndex)
        {
            return (await _redisDatabase.SortedSetRangeByRankAsync(key.ToLower(), startIndex, endIndex)).Select(x => x.ToString());
        }

        public async Task<IEnumerable<string>> GetSortedSetElementsByValue(string key, string minValue, string maxValue)
        {
            if (string.IsNullOrEmpty(maxValue)) maxValue = default(RedisValue);
            if (string.IsNullOrEmpty(minValue)) minValue = default(RedisValue);
            return (await _redisDatabase.SortedSetRangeByValueAsync(key.ToLower(), minValue, maxValue)).Select(x => x.ToString());
        }

        public async Task<bool> AddToSortedSet<T>(string key, T value, double score = 0D)
        {
            return await _redisDatabase.SortedSetAddAsync(key.ToLower(), Helpers.SerializeRedisValue(value), score);
        }

        public async Task<bool> RemoveFromSortedSetByValue(string key, string value)
        {
            return await _redisDatabase.SortedSetRemoveAsync(key.ToLower(), value);
        }
    }
}