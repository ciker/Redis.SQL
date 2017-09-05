using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Interfaces;
using StackExchange.Redis;

namespace Redis.SQL.Client
{
    internal class RedisStorageClient : IStorageClient
    {
        private readonly IDatabase _redisDatabase;

        internal RedisStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }

        #region Strings
        public async Task<T> GetValue<T>(string key)
        {
            var value = await _redisDatabase.StringGetAsync(key.ToLower());
            return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<bool> StoreValue<T>(string key, T value)
        {
            return await _redisDatabase.StringSetAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }

        public async Task<long> IncrementValue(string key)
        {
            return await _redisDatabase.StringIncrementAsync(key.ToLower());
        }
        #endregion

        #region HashSets
        public async Task<T> GetHashField<T>(string hashSet, string key)
        {
            var value = await _redisDatabase.HashGetAsync(hashSet.ToLower(), key.ToLower());
            return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<bool> StoreHashField<T>(string hashSet, string key, T value)
        {
            return await _redisDatabase.HashSetAsync(hashSet.ToLower(), key.ToLower(), JsonConvert.SerializeObject(value));
        }
        #endregion

        #region Lists
        public async Task<string> GetListElementByIndex(string key, int index)
        {
            return await _redisDatabase.ListGetByIndexAsync(key.ToLower(), index);
        }

        public async Task<long> AddToList<T>(string key, T value)
        {
            return await _redisDatabase.ListRightPushAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }

        #endregion

        #region Sets
        public async Task<bool> SetContains<T>(string key, T value)
        {
            return await _redisDatabase.SetContainsAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }

        public async Task<bool> AddToSet<T>(string key, T value)
        {
            return await _redisDatabase.SetAddAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }
        #endregion

        #region Sorted Sets
        public async Task<IEnumerable<string>> GetSortedSetElementsByScore(string key, double minScore, double maxScore)
        {
            return (await _redisDatabase.SortedSetRangeByScoreAsync(key.ToLower(), minScore, maxScore)).Select(x => x.ToString());
        }

        public async Task<bool> AddToSortedSet<T>(string key, T value, double score)
        {
            return await _redisDatabase.SortedSetAddAsync(key.ToLower(), JsonConvert.SerializeObject(value), score);
        }
        #endregion
    }
}