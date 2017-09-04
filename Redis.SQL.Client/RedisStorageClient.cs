using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Redis.SQL.Client
{
    public class RedisStorageClient
    {
        private readonly IDatabase _redisDatabase;

        public RedisStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }

        public async Task<T> GetValue<T>(string key)
        {
            var value = await _redisDatabase.StringGetAsync(key.ToLower());
            return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<bool> StoreValue<T>(string key, T value)
        {
            return await _redisDatabase.StringSetAsync(key.ToLower(), JsonConvert.SerializeObject(value));
        }

        public async Task<bool> StoreHashField<T>(string hashSet, string key, T value)
        {
            return await _redisDatabase.HashSetAsync(hashSet.ToLower(), key.ToLower(), JsonConvert.SerializeObject(value));
        }
        
        public async Task<T> GetHashField<T>(string hashSet, string key)
        {
            var value = await _redisDatabase.HashGetAsync(hashSet.ToLower(), key.ToLower());
            return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}