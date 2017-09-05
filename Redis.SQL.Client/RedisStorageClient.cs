using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Interfaces;
using StackExchange.Redis;

namespace Redis.SQL.Client
{
    internal class RedisStorageClient : IRedisStringStorageClient, IRedisHashStorageClient, IRedisListStorageClient, IRedisSetStorageClient, IRedisZSetStorageClient
    {
        private readonly IDatabase _redisDatabase;

        private static readonly SemaphoreSlim Mutex = new SemaphoreSlim(1, 1);

        internal RedisStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }

        #region Strings
        public async Task<string> GetValue(string key)
        {
            return await _redisDatabase.StringGetAsync(key.ToLower());
        }

        public async Task<bool> StoreValue<T>(string key, T value)
        {
            return await _redisDatabase.StringSetAsync(key.ToLower(), GetRedisValue(value));
        }

        public async Task<long> IncrementValue(string key)
        {
            return await _redisDatabase.StringIncrementAsync(key.ToLower());
        }
        #endregion

        #region HashSets
        public async Task<string> GetHashField(string hashSet, string key)
        {
            return await _redisDatabase.HashGetAsync(hashSet.ToLower(), key.ToLower());
        }

        public async Task<bool> SetHashField<T>(string hashSet, string key, T value)
        {
            return await _redisDatabase.HashSetAsync(hashSet.ToLower(), key.ToLower(), GetRedisValue(value));
        }

        public async Task<bool> AppendStringToHashField(string hashSet, string key, string value)
        {
            await Mutex.WaitAsync();

            try
            {
                var field = await GetHashField(hashSet, key);

                if (string.IsNullOrEmpty(field))
                {
                    field = value;
                }
                else if (field.Split(',').All(x => x != value))
                {
                    field += "," + value;
                }

                return await SetHashField(hashSet.ToLower(), key.ToLower(), GetRedisValue(field));
            }
            finally
            {
                Mutex.Release();
            }
        }

        public async Task<bool> RemoveStringFromHashField(string hashSet, string key, string value)
        {
            await Mutex.WaitAsync();

            try
            {
                var field = await GetHashField(hashSet, key);

                if (!string.IsNullOrEmpty(field))
                {
                    field = string.Join(",", field.Split(',').Where(x => x != value));
                }

                return await SetHashField(hashSet.ToLower(), key.ToLower(), GetRedisValue(field));
            }
            finally
            {
                Mutex.Release();
            }
        }
        #endregion

        #region Lists
        public async Task<string> GetListElementByIndex(string key, int index)
        {
            return await _redisDatabase.ListGetByIndexAsync(key.ToLower(), index);
        }

        public async Task<long> AddToListTail<T>(string key, T value)
        {
            return await _redisDatabase.ListRightPushAsync(key.ToLower(), GetRedisValue(value));
        }

        public async Task<long> AddToListHead<T>(string key, T value)
        {
            return await _redisDatabase.ListLeftPushAsync(key.ToLower(), GetRedisValue(value));
        }
        #endregion

        #region Sets
        public async Task<bool> SetContains<T>(string key, T value)
        {
            return await _redisDatabase.SetContainsAsync(key.ToLower(), GetRedisValue(value));
        }

        public async Task<bool> AddToSet<T>(string key, T value)
        {
            return await _redisDatabase.SetAddAsync(key.ToLower(), GetRedisValue(value));
        }
        #endregion

        #region Sorted Sets
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
            return (await _redisDatabase.SortedSetRangeByValueAsync(key.ToLower(), minValue, maxValue)).Select(x => x.ToString());
        }

        public async Task<bool> AddToSortedSet<T>(string key, T value, double score = 0D)
        {
            return await _redisDatabase.SortedSetAddAsync(key.ToLower(), GetRedisValue(value), score);
        }
        #endregion

        private static string GetRedisValue<T>(T value) => typeof(T) == typeof(string) ? value.ToString() : JsonConvert.SerializeObject(value);
    }
}