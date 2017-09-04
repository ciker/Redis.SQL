using System.Reflection;
using System.Threading.Tasks;
using Redis.SQL.Client.Interfaces;

namespace Redis.SQL.Client
{
    public class RedisSqlEngine
    {
        private readonly IStorageClient _client;

        public RedisSqlEngine()
        {
            _client = new RedisStorageClient();
        }

        public async Task CreateEntity<TEntity>(TEntity entity) where TEntity : class
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var identifier = Helpers.GenerateRandomString();

            await _client.StoreValue(Helpers.GetEntityStoreKey(entityName, identifier), entity);
            await _client.IncrementValue(Helpers.GetEntityCountKey(entityName));

            var properties = typeof(TEntity).GetProperties();
            foreach (var property in properties)
            {
                var fieldValue = property.GetValue(entity).ToString();
                await _client.StoreHashField(Helpers.GetEntityIndexKey(entityName, property.Name), fieldValue, identifier);
            }
        }
    }
}
