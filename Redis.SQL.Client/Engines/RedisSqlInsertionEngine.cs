using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.Enums;
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
            var tokens = _insertionLexer.Tokenize(statement);
            var model = (InsertionModel)_insertionParser.ParseTokens(tokens);
            var identifier = Helpers.GenerateRandomString();
            var entity = await EncodeEntity(model.EntityName, identifier, model.PropertyValues);
            await AddEntityToStore(model.EntityName, identifier, entity);
        }
        
        public async Task InsertEntity<TEntity>(TEntity entity)
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var identifier = Helpers.GenerateRandomString();
            var properties = Helpers.GetTypeProperties<TEntity>();

            await AddEntityToStore(entityName, identifier, entity);

            foreach (var property in properties)
            {
                var propertyTypeName = GetPropertyTypeName(property);
                var propertyValue = GetPropertyValue(property, entity).ToString();
                await AddPropertyToStore(entityName, identifier, propertyTypeName, property.Name, propertyValue);
            }
        }
        
        private async Task AddPropertyToStore(string entityName, string identifier, string propertyTypeName, string propertyName, string propertyValue)
        {
            VerifyValueType(propertyTypeName, propertyValue);
            var encodedPropertyValue = Helpers.EncodePropertyValue(propertyTypeName, propertyValue).ToLower();
            var propertyScore = Helpers.GetPropertyScore(propertyTypeName, encodedPropertyValue);
            await _hashClient.AppendStringToHashField(Helpers.GetEntityIndexKey(entityName, propertyName), encodedPropertyValue, identifier);
            await _zSetClient.AddToSortedSet(Helpers.GetPropertyCollectionKey(entityName, propertyName), encodedPropertyValue, propertyScore ?? 0D);
        }

        private async Task AddEntityToStore<TEntity>(string entityName, string identifier, TEntity entity)
        {
            await CheckEntityExistance(entityName);
            await _setClient.AddToSet(Helpers.GetEntityIdentifierCollectionKey(entityName), identifier);
            await _stringClient.StoreValue(Helpers.GetEntityStoreKey(entityName, identifier), entity);
            await _stringClient.IncrementValue(Helpers.GetEntityCountKey(entityName));
        }

        private async Task<string> EncodeEntity(string entityName, string identifier, IDictionary<string, string> propertyValues)
        {
            var entity = string.Empty;
            foreach (var item in propertyValues)
            {
                var propertyType = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), item.Key.ToLower());
                var value = item.Value;

                await AddPropertyToStore(entityName, identifier, propertyType, item.Key, item.Value);

                if (Enum.TryParse(propertyType, true, out TypeNames type))
                {
                    if (type == TypeNames.Char || type == TypeNames.DateTime || type == TypeNames.String || type == TypeNames.TimeSpan)
                    {
                        value = $"\"{item.Value}\"";
                    }
                }
                else
                {
                    throw new UnsupportedTypeException(propertyType);
                }

                entity += $"  \"{item.Key}\": {value}, {Environment.NewLine}";
            }

            return "{" + Environment.NewLine + entity + "}";
        }

        private async Task CheckEntityExistance(string entityName)
        {
            var mutex = Semaphores.GetEntitySemaphore(entityName);
            await mutex.WaitAsync();

            try
            {
                if (!await _setClient.SetContains(Constants.AllEntityNamesSetKeyName, entityName))
                {
                    throw new EntityNotFoundException(entityName);
                }
            }
            finally
            {
                mutex.Release();
            }
        }

        private static void VerifyValueType(string propertyType, string value)
        {
            if (Enum.TryParse(propertyType, true, out TypeNames type))
            {
                switch (type)
                {
                    case TypeNames.DateTime when !DateTime.TryParse(value, new CultureInfo("en-US"), DateTimeStyles.None, out var _):
                        throw new IncompatibleTypesException(TypeNames.DateTime.ToString(), value);

                    case TypeNames.Boolean when !bool.TryParse(value, out var _):
                        throw new IncompatibleTypesException(TypeNames.Boolean.ToString(), value);

                    case TypeNames.TimeSpan when !TimeSpan.TryParse(value, out var _):
                        throw new IncompatibleTypesException(TypeNames.TimeSpan.ToString(), value);

                    case TypeNames.Int32 when !int.TryParse(value, out var _):
                        throw new IncompatibleTypesException(TypeNames.Int32.ToString(), value);

                    case TypeNames.Int64 when !long.TryParse(value, out var _):
                        throw new IncompatibleTypesException(TypeNames.Int64.ToString(), value);

                    case TypeNames.Char when !char.TryParse(value, out var _):
                        throw new IncompatibleTypesException(TypeNames.Char.ToString(), value);
                }
            }
        }

        private static string GetPropertyTypeName(PropertyInfo property) => property.PropertyType.Name;

        private static object GetPropertyValue<TEntity>(PropertyInfo property, TEntity entity) => property.GetValue(entity);
    }
}