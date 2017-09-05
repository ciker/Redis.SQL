using System.Threading.Tasks;
using Redis.SQL.Client.Interfaces;

namespace Redis.SQL.Client
{
    public class RedisSqlEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisStringStorageClient _stringClient;

        public RedisSqlEngine()
        {
            _hashClient = new RedisStorageClient();
            _stringClient = new RedisStorageClient();
        }

        public async Task CreateEntity<TEntity>(TEntity entity) where TEntity : class
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var identifier = Helpers.GenerateRandomString();
            var properties = Helpers.GetTypeProperties<TEntity>();

            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(entityName, identifier), entity);
            await _stringClient.IncrementValue(Helpers.GetEntityCountKey(entityName));

            foreach (var property in properties)
            {
                var fieldValue = property.GetValue(entity).ToString();
                await _hashClient.StoreHashField(Helpers.GetEntityIndexKey(entityName, property.Name), fieldValue, identifier);
            }
        }
    }
}