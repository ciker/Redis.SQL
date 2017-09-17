using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlDeletionEngine : IDeletionEngine
    {
        private readonly IQueryEngine _queryEngine;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisZSetStorageClient _zSetClient;
        private readonly IRedisSetStorageClient _setClient;

        internal RedisSqlDeletionEngine()
        {
            _stringClient = new RedisStringStorageClient();
            _queryEngine = new RedisSqlQueryEngine();
            _hashClient = new RedisHashStorageClient();
            _zSetClient = new RedisZSetStorageClient();
            _setClient = new RedisSetStorageClient();
        }

        public async Task ExecuteDeleteStatement(string statement)
        {
            await DeleteEntityByKey("user", "f30ec0d6b643427ab628393c652166ee");
        }

        private async Task DeleteEntityByKey(string entityName, string key)
        {
            var entityStoreKey = Helpers.GetEntityStoreKey(entityName, key);

            var encodedEntity = await _stringClient.GetValue(entityStoreKey);

            var entity = JsonConvert.DeserializeObject<dynamic>(encodedEntity);

            var propertyValues = new Dictionary<string, string>();

            foreach (var property in entity)
            {
                var name = property.Name;
                var propertyTypeName = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), name);
                var value = Helpers.EncodePropertyValue(propertyTypeName, (property as IEnumerable<dynamic>)?.FirstOrDefault()?.Value?.ToString());
                propertyValues.Add(name, value);
            }

            await _stringClient.DeleteValue(entityStoreKey);
            await _stringClient.DecrementValue(Helpers.GetEntityCountKey(entityName));
            await _setClient.RemoveFromSetByValue(Helpers.GetEntityIdentifierCollectionKey(entityName), key);

            foreach (var valueMap in propertyValues)
            {
                var entityPropertyIndexKey = Helpers.GetEntityIndexKey(entityName, valueMap.Key);
                await _hashClient.RemoveStringFromHashField(entityPropertyIndexKey, valueMap.Value, key);
                var updatedIndex = await _hashClient.GetHashField(entityPropertyIndexKey, valueMap.Value);

                if (string.IsNullOrWhiteSpace(updatedIndex))
                {
                    await _zSetClient.RemoveFromSortedSetByValue(Helpers.GetPropertyCollectionKey(entityName, valueMap.Key), valueMap.Value);
                }
            }
        }
    }
}