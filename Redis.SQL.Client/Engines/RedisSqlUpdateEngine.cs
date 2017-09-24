using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.Models;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlUpdateEngine : IUpdateEngine
    {
        private readonly IQueryEngine _queryEngine;
        private readonly IDeletionEngine _deltionEngine;
        private readonly IInsertionEngine _insertionEngine;
        private readonly ILexer _updateLexicalTokenizer;
        private readonly ICustomizedParser _updateParser;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisHashStorageClient _hashClient;

        internal RedisSqlUpdateEngine()
        {
            _updateLexicalTokenizer = new UpdateLexicalTokenizer();
            _updateParser = new UpdateParser();
            _queryEngine = new RedisSqlQueryEngine();
            _stringClient = new RedisStringStorageClient();
            _hashClient = new RedisHashStorageClient();
            _deltionEngine = new RedisSqlDeletionEngine();
            _insertionEngine = new RedisSqlInsertionEngine();
        }

        public async Task ExecuteUpdateStatement(string sql)
        {
            var tokens = _updateLexicalTokenizer.Tokenize(sql);
            var model = (UpdateModel)_updateParser.ParseTokens(tokens);
            var updatedKeys = await _queryEngine.GetEntityKeys(model.EntityName, model.WhereCondition);
            foreach (var key in updatedKeys)
            {
                var entityStoreKey = Helpers.GetEntityStoreKey(model.EntityName, key);
                var encodedEntity = await _stringClient.GetValue(entityStoreKey);
                var entity = JsonConvert.DeserializeObject<dynamic>(encodedEntity);
                await UpdateWeakEntity(entity, model.UpdatedProperties, model.EntityName, key);
            }
        }

        public async Task UpdateEntity<TEntity>(TEntity entity)
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var condition = await _queryEngine.ConstructWhereStatementFromIdentifiers(entity);
            var updatedKeys = (await _queryEngine.GetEntityKeys(entityName, condition)).ToList();

            if (updatedKeys.Count > 1)
            {
                throw new NonUniqueKeyException();
            }

            if (updatedKeys.Count == 0)
            {
                return;
            }

            var key = updatedKeys[0];
            await UpdateStrongEntity(entity, key, entityName);
        }

        private async Task UpdateStrongEntity<TEntity>(TEntity updatedEntity, string key, string entityName)
        {
            var storedEntity = await _queryEngine.GetEntityByKey<TEntity>(key);
            var updatedEntityProps = new Dictionary<string, string>();
            var entityType = storedEntity.GetType();
            var props = entityType.GetProperties();

            foreach (var prop in props)
            {
                var propertyName = prop.Name.ToLower();
                var storedValue = prop.GetValue(storedEntity).ToString();
                var newValue = prop.GetValue(updatedEntity).ToString();
                if (!Equals(storedValue, newValue))
                {
                    await UpdateProperty(propertyName, storedValue, newValue, entityName, key);
                    updatedEntityProps.Add(propertyName, newValue);
                }
                else
                {
                    updatedEntityProps.Add(propertyName, storedValue);
                }
            }

            await UpdateEntityStore(entityName, key, updatedEntityProps);
        }

        private async Task UpdateWeakEntity(dynamic entity, IDictionary<string, string> updatedProps, string entityName, string key)
        {
            var updatedEntityProps = new Dictionary<string, string>();
            foreach (var property in entity)
            {
                var name = property.Name.ToLower();
                var value = (property as IEnumerable<dynamic>)?.FirstOrDefault()?.Value?.ToString();

                if (updatedProps.TryGetValue(name.ToLower(), out string updatedValue))
                {
                    updatedValue = updatedValue.Trim(' ', '\'');
                    await UpdateProperty(name, value, updatedValue, entityName, key);
                    updatedEntityProps.Add(name, updatedValue);
                }
                else
                {
                    updatedEntityProps.Add(name, value);
                }
            }

            await UpdateEntityStore(entityName, key, updatedEntityProps);
        }

        private async Task UpdateProperty(string propertyName, string oldValue, string newValue, string entityName, string key)
        {
            var propertyTypeName = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), propertyName);
            await _deltionEngine.PurgeProperty(entityName, key, propertyName, oldValue, propertyTypeName);
            await _insertionEngine.AddPropertyToStore(entityName, key, propertyTypeName, propertyName, newValue);
        }

        private async Task UpdateEntityStore(string entityName, string key, IDictionary<string, string> updatedEntityProps)
        {
            var updatedEntity = await _insertionEngine.EncodeEntity(entityName, key, updatedEntityProps);
            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(entityName, key), updatedEntity);
        }
    }
}