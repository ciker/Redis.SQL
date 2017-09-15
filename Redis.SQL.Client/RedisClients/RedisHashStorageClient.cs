using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.RedisClients.Interfaces;
using StackExchange.Redis;

namespace Redis.SQL.Client.RedisClients
{
    internal class RedisHashStorageClient : IRedisHashStorageClient
    {
        private readonly IDatabase _redisDatabase;

        internal RedisHashStorageClient()
        {
            var configurationManager = new ConfigurationManager();

            var indexConfiguration = configurationManager.GetConfigKey(Constants.RedisDatabaseIndex);

            var databaseIndex = string.IsNullOrEmpty(indexConfiguration) ? Constants.DefaultDatabaseIndex : int.Parse(indexConfiguration);

            _redisDatabase = RedisConnectionMultiplexer.GetMultiplexer().Connection.GetDatabase(databaseIndex);
        }

        public async Task<string> GetHashField(string hashSet, string key)
        {
            return await _redisDatabase.HashGetAsync(hashSet.ToLower(), key.ToLower());
        }
        
        public async Task<bool> SetHashField<T>(string hashSet, string key, T value)
        {
            return await _redisDatabase.HashSetAsync(hashSet.ToLower(), key.ToLower(), Helpers.SerializeRedisValue(value));
        }

        public async Task<bool> AppendStringToHashField(string hashSet, string key, string value)
        {
            var mutex = Semaphores.GetIndexSemaphore(hashSet, key);
            await mutex.WaitAsync();

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

                return await SetHashField(hashSet.ToLower(), key.ToLower(), Helpers.SerializeRedisValue(field));
            }
            finally
            {
                mutex.Release();
            }
        }

        public async Task<bool> RemoveStringFromHashField(string hashSet, string key, string value)
        {
            var mutex = Semaphores.GetIndexSemaphore(hashSet, key);
            await mutex.WaitAsync();

            try
            {
                var field = await GetHashField(hashSet, key);

                if (!string.IsNullOrEmpty(field))
                {
                    field = string.Join(",", field.Split(',').Where(x => x != value));
                }

                return await SetHashField(hashSet.ToLower(), key.ToLower(), Helpers.SerializeRedisValue(field));
            }
            finally
            {
                mutex.Release();
            }
        }
    }
}