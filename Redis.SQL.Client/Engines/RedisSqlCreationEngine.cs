using System.Threading.Tasks;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlCreationEngine : ICreationEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisSetStorageClient _setClient;
        private readonly IRedisStringStorageClient _stringClient;

        internal RedisSqlCreationEngine()
        {
            _hashClient = new RedisHashStorageClient();
            _setClient = new RedisSetStorageClient();
            _stringClient = new RedisStringStorageClient();
        }

        public async Task<bool> CreateEntity<TEntity>()
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var properties = Helpers.GetTypeProperties<TEntity>();
            var mutex = Semaphores.GetEntitySemaphore(entityName);
            await mutex.WaitAsync();

            try
            {
                if (await _setClient.SetContains(Constants.AllEntityNamesSetKeyName, entityName))
                {
                    return false;
                }

                foreach (var property in properties)
                {
                    await _hashClient.SetHashField(Helpers.GetEntityPropertyTypesKey(entityName), property.Name, property.PropertyType.Name.ToLower());
                }

                await _setClient.AddToSet(Constants.AllEntityNamesSetKeyName, entityName);
                await _stringClient.StoreValue(Helpers.GetEntityCountKey(entityName), 0.ToString());

                return true;
            }
            finally
            {
                mutex.Release();
            }
        }
    }
}