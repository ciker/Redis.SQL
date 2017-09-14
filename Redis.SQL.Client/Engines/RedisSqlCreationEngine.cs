using System.Reflection;
using System.Threading.Tasks;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlCreationEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisZSetStorageClient _zSetClient;
        private readonly IRedisSetStorageClient _setClient;

        internal RedisSqlCreationEngine()
        {
            _hashClient = new RedisHashStorageClient();
            _stringClient = new RedisStringStorageClient();
            _zSetClient = new RedisZSetStorageClient();
            _setClient = new RedisSetStorageClient();
        }

        internal async Task CreateEntity<TEntity>(TEntity entity) where TEntity : class
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var identifier = Helpers.GenerateRandomString();
            var properties = Helpers.GetTypeProperties<TEntity>();

            await _setClient.AddToSet(Helpers.GetEntityIdentifierCollectionKey(entityName), identifier);
            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(entityName, identifier), entity);

            foreach (var property in properties)
            {
                var propertyTypeName = GetPropertyTypeName(property);
                var propertyValue = GetPropertyValue(property, entity);
                var encodedPropertyValue = Helpers.EncodePropertyValue(propertyTypeName, propertyValue.ToString()).ToLower();
                var propertyScore = Helpers.GetPropertyScore(propertyTypeName, encodedPropertyValue);
                await _hashClient.SetHashField(Helpers.GetEntityPropertyTypesKey(entityName), property.Name, property.PropertyType.Name);
                await _hashClient.AppendStringToHashField(Helpers.GetEntityIndexKey(entityName, property.Name), encodedPropertyValue, identifier);
                await _zSetClient.AddToSortedSet(Helpers.GetPropertyCollectionKey(entityName, property.Name), encodedPropertyValue, propertyScore ?? 0D);
            }

            await _setClient.AddToSet(Constants.AllEntityNamesSetKeyName, entityName);
            await _stringClient.IncrementValue(Helpers.GetEntityCountKey(entityName));
        }
        
        private static string GetPropertyTypeName(PropertyInfo property) => property.PropertyType.Name;

        private static object GetPropertyValue<TEntity>(PropertyInfo property, TEntity entity) => property.GetValue(entity);
    }
}