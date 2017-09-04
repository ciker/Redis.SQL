using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Redis.SQL.Client
{
    internal class RedisStorageClient
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
        internal async Task<T> GetValue<T>(string key)
        {
            var value = await _redisDatabase.StringGetAsync(key.ToLower());
            return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        internal async Task<bool> StoreValue<T>(string key, T value)
        {
            return await _redisDatabase.StringSetAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }
        #endregion

        #region HashSets
        internal async Task<T> GetHashField<T>(string hashSet, string key)
        {
            var value = await _redisDatabase.HashGetAsync(hashSet.ToLower(), key.ToLower());
            return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        internal async Task<bool> StoreHashField<T>(string hashSet, string key, T value)
        {
            return await _redisDatabase.HashSetAsync(hashSet.ToLower(), key.ToLower(), JsonConvert.SerializeObject(value));
        }
        #endregion

        #region Sets
        internal async Task<bool> SetContains<T>(string key, T value)
        {
            return await _redisDatabase.SetContainsAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }

        internal async Task<bool> AddToSet<T>(string key, T value)
        {
            return await _redisDatabase.SetAddAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }
        #endregion
    }
}