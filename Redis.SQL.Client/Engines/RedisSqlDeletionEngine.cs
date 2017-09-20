using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.Models;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlDeletionEngine : IDeletionEngine
    {
        private readonly IQueryEngine _queryEngine;
        private readonly ILexer _deletionLexicalTokenizer;
        private readonly ICustomizedParser _deletionParser;
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
            _deletionLexicalTokenizer = new DeletionLexicalTokenizer();
            _deletionParser = new DeletionParser();
        }

        public async Task ExecuteDeleteStatement(string statement)
        {
            var tokens = _deletionLexicalTokenizer.Tokenize(statement);
            var model = (DeletionModel)_deletionParser.ParseTokens(tokens);
            var keys = await _queryEngine.QueryKeys(model.EntityName, model.WhereCondition);
            foreach (var key in keys)
            {
                await DeleteEntityByKey(model.EntityName, key);
            }
        }

        private async Task DeleteEntityByKey(string entityName, string key)
        {
            var entityStoreKey = Helpers.GetEntityStoreKey(entityName, key);

            var encodedEntity = await _stringClient.GetValue(entityStoreKey);

            var entity = JsonConvert.DeserializeObject<dynamic>(encodedEntity);

            await _stringClient.DeleteValue(entityStoreKey);
            await _stringClient.DecrementValue(Helpers.GetEntityCountKey(entityName));
            await _setClient.RemoveFromSetByValue(Helpers.GetEntityIdentifierCollectionKey(entityName), key);

            foreach (var property in entity)
            {
                var name = property.Name;
                var propertyTypeName = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), name);
                var value = Helpers.EncodePropertyValue(propertyTypeName, (property as IEnumerable<dynamic>)?.FirstOrDefault()?.Value?.ToString());
                await PurgeProperty(entityName, key, name, value);
            }
        }

        private async Task PurgeProperty(string entityName, string entityKey, string propertyName, string propertyValue)
        {
            var entityPropertyIndexKey = Helpers.GetEntityIndexKey(entityName, propertyName);
            await _hashClient.RemoveStringFromHashField(entityPropertyIndexKey, propertyValue, entityKey);
            var updatedIndex = await _hashClient.GetHashField(entityPropertyIndexKey, propertyValue);

            if (string.IsNullOrWhiteSpace(updatedIndex))
            {
                await _zSetClient.RemoveFromSortedSetByValue(Helpers.GetPropertyCollectionKey(entityName, propertyName), propertyValue.ToLower());
            }
        }
    }
}