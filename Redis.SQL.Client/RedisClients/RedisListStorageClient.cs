using System.Threading.Tasks;
using Redis.SQL.Client.RedisClients.Interfaces;
using StackExchange.Redis;

namespace Redis.SQL.Client.RedisClients
{
    internal class RedisListStorageClient : IRedisListStorageClient
    {
        private readonly IDatabase _redisDatabase;

        internal RedisListStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }

        public async Task<string> GetListElementByIndex(string key, int index)
        {
            return await _redisDatabase.ListGetByIndexAsync(key.ToLower(), index);
        }

        public async Task<long> AddToListTail<T>(string key, T value)
        {
            return await _redisDatabase.ListRightPushAsync(key.ToLower(), Helpers.SerializeRedisValue(value));
        }

        public async Task<long> AddToListHead<T>(string key, T value)
        {
            return await _redisDatabase.ListLeftPushAsync(key.ToLower(), Helpers.SerializeRedisValue(value));
        }
    }
}