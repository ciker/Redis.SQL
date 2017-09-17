using System.Threading.Tasks;
using Redis.SQL.Client.RedisClients.Interfaces;
using StackExchange.Redis;

namespace Redis.SQL.Client.RedisClients
{
    internal class RedisStringStorageClient : IRedisStringStorageClient
    {
        private readonly IDatabase _redisDatabase;

        internal RedisStringStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }
        
        public async Task<string> GetValue(string key)
        {
            return await _redisDatabase.StringGetAsync(key.ToLower());
        }

        public async Task<bool> StoreValue<T>(string key, T value)
        {
            return await _redisDatabase.StringSetAsync(key.ToLower(), Helpers.SerializeRedisValue(value));
        }

        public async Task<long> IncrementValue(string key)
        {
            return await _redisDatabase.StringIncrementAsync(key.ToLower());
        }

        public async Task<long> DecrementValue(string key)
        {
            return await _redisDatabase.StringDecrementAsync(key.ToLower());
        }

        public async Task<bool> DeleteValue(string key)
        {
            return await _redisDatabase.KeyDeleteAsync(key.ToLower());
        }
    }
}