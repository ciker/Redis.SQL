using System.Collections.Generic;
using System.Dynamic;
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
    internal class RedisSqlInsertionEngine : IInsertionEngine
    {
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisZSetStorageClient _zSetClient;
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisSetStorageClient _setClient;
        private readonly ILexer _insertionLexer;
        private readonly ICustomizedParser _insertionParser;

        internal RedisSqlInsertionEngine()
        {
            _hashClient = new RedisHashStorageClient();
            _stringClient = new RedisStringStorageClient();
            _zSetClient = new RedisZSetStorageClient();
            _setClient = new RedisSetStorageClient();
            _insertionLexer = new InsertionLexicalTokenizer();
            _insertionParser = new InsertionParser();
        }

        public async Task ExecuteInsertStatement(string statement)
        {
            var tokens = _insertionLexer.Tokenize(statement).ToList();
            var model = (InsertionModel)_insertionParser.ParseTokens(tokens);
            var identifier = Helpers.GenerateRandomString();

            if (!await CheckEntityExistance(model.EntityName))
            {
                throw new EntityNotFoundException(model.EntityName);
            }

            await _setClient.AddToSet(Helpers.GetEntityIdentifierCollectionKey(model.EntityName), identifier);


            var json = JsonConvert.SerializeObject(model.PropertyValues, Formatting.Indented);

            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(model.EntityName, identifier), json);
        }
        
        public async Task InsertEntity<TEntity>(TEntity entity)
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var identifier = Helpers.GenerateRandomString();
            var properties = Helpers.GetTypeProperties<TEntity>();

            if (!await CheckEntityExistance(entityName))
            {
                throw new EntityNotFoundException(entityName);
            }

            await _setClient.AddToSet(Helpers.GetEntityIdentifierCollectionKey(entityName), identifier);
            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(entityName, identifier), entity);

            foreach (var property in properties)
            {
                var propertyTypeName = GetPropertyTypeName(property);
                var propertyValue = GetPropertyValue(property, entity);
                var encodedPropertyValue = Helpers.EncodePropertyValue(propertyTypeName, propertyValue.ToString()).ToLower();
                var propertyScore = Helpers.GetPropertyScore(propertyTypeName, encodedPropertyValue);
                await _hashClient.AppendStringToHashField(Helpers.GetEntityIndexKey(entityName, property.Name), encodedPropertyValue, identifier);
                await _zSetClient.AddToSortedSet(Helpers.GetPropertyCollectionKey(entityName, property.Name), encodedPropertyValue, propertyScore ?? 0D);
            }

            await _stringClient.IncrementValue(Helpers.GetEntityCountKey(entityName));
        }

        private async Task<bool> CheckEntityExistance(string entityName)
        {
            var mutex = Semaphores.GetEntitySemaphore(entityName);
            await mutex.WaitAsync();

            try
            {
                return await _setClient.SetContains(Constants.AllEntityNamesSetKeyName, entityName);
            }
            finally
            {
                mutex.Release();
            }
        }

        private static string GetPropertyTypeName(PropertyInfo property) => property.PropertyType.Name;

        private static object GetPropertyValue<TEntity>(PropertyInfo property, TEntity entity) => property.GetValue(entity);
    }
}
