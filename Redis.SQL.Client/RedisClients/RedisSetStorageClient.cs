using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.RedisClients.Interfaces;
using StackExchange.Redis;

namespace Redis.SQL.Client.RedisClients
{
    internal class RedisSetStorageClient : IRedisSetStorageClient
    {
        private readonly IDatabase _redisDatabase;

        internal RedisSetStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }

        public async Task<bool> SetContains<T>(string key, T value)
        {
            return await _redisDatabase.SetContainsAsync(key.ToLower(), Helpers.SerializeRedisValue(value));
        }

        public async Task<bool> AddToSet<T>(string key, T value)
        {
            return await _redisDatabase.SetAddAsync(key.ToLower(), Helpers.SerializeRedisValue(value));
        }

        public async Task<IEnumerable<string>> GetSetMembers(string key)
        {
            return (await _redisDatabase.SetMembersAsync(key.ToLower())).Select(x => x.ToString());
        }

        public async Task<bool> RemoveFromSetByValue(string key, string value)
        {
            return await _redisDatabase.SetRemoveAsync(key.ToLower(), value);
        }
    }
}