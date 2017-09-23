using System;
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

                await UpdateEntity(entity, model.UpdatedProperties, model.EntityName, key);
            }
        }

        private async Task UpdateEntity(dynamic entity, IDictionary<string, string> updatedProps, string entityName, string key)
        {
            var updatedEntityProps = new Dictionary<string, string>();
            foreach (var property in entity)
            {
                var name = property.Name;
                var propertyTypeName = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), name);
                var value = (property as IEnumerable<dynamic>)?.FirstOrDefault()?.Value?.ToString();
                
                if (updatedProps.Any(x => string.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase)))
                {
                    await _deltionEngine.PurgeProperty(entityName, key, name, value, propertyTypeName);
                    await _insertionEngine.AddPropertyToStore(entityName, key, propertyTypeName, name, value);
                }

                updatedEntityProps.Add(name, value);
            }

            var updatedEntity = await _insertionEngine.EncodeEntity(entityName, key, updatedEntityProps);
            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(entityName, key), updatedEntity);
        }
    }
}