using System;
using System.Reflection;
using System.Threading.Tasks;
using Redis.SQL.Client.Interfaces;

namespace Redis.SQL.Client
{
    public class RedisSqlEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisZSetStorageClient _zSetClient;

        public RedisSqlEngine()
        {
            _hashClient = new RedisStorageClient();
            _stringClient = new RedisStorageClient();
            _zSetClient = new RedisStorageClient();
        }

        public async Task CreateEntity<TEntity>(TEntity entity) where TEntity : class
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var identifier = Helpers.GenerateRandomString();
            var properties = Helpers.GetTypeProperties<TEntity>();

            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(entityName, identifier), entity);

            foreach (var property in properties)
            {
                var propertyValue = GetPropertyRedisValue(property, entity);
                await _hashClient.StoreHashField(Helpers.GetEntityIndexKey(entityName, property.Name), propertyValue, identifier);
                await _zSetClient.AddToSortedSet(Helpers.GetPropertyCollectionKey(entityName, property.Name), propertyValue);
            }

            await _stringClient.IncrementValue(Helpers.GetEntityCountKey(entityName));
        }

        private static string GetPropertyRedisValue<TEntity>(PropertyInfo property, TEntity entity)
        {
            var value = property.GetValue(entity);

            if (property.PropertyType == typeof(DateTime))
            {
                return Helpers.GetDateTimeRedisValue((DateTime)value);
            }

            if (property.PropertyType == typeof(TimeSpan))
            {
                return Helpers.GetTimeSpanRedisValue((TimeSpan)value);
            }

            if (property.PropertyType == typeof(bool))
            {
                return Helpers.GetBooleanRedisValue((bool)value);
            }

            return value.ToString();
        }
    }
}