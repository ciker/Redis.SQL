using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal partial class RedisSqlQueryEngine : IQueryEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisZSetStorageClient _zSetClient;
        private readonly IRedisSetStorageClient _setClient;
        private readonly ILexer _conditionalTokenizer;
        private readonly IShiftReduceParser _whereParser;
        private static readonly string[] Operators = {"<=", ">=", "<", ">", "!=", "="};

        internal RedisSqlQueryEngine()
        {
            _hashClient = new RedisHashStorageClient();
            _stringClient = new RedisStringStorageClient();
            _zSetClient = new RedisZSetStorageClient();
            _setClient = new RedisSetStorageClient();
            _conditionalTokenizer = new ConditionalLexicalTokenizer();
            _whereParser = new ShiftReduceParser(Constants.WhereGrammar);
        }

        public async Task<IEnumerable<TEntity>> QueryEntities<TEntity>(string condition)
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var entities = await GetSerializedEntities(entityName, condition);
            return entities.Select(JsonConvert.DeserializeObject<TEntity>).ToList();
        }

        public async Task<TEntity> GetEntityByKey<TEntity>(string key)
        {
            var entityName = Helpers.GetTypeName<TEntity>();

            var entityJson = await RetrieveEntityJsonByKey(entityName, key);

            return JsonConvert.DeserializeObject<TEntity>(entityJson);
        }

        public async Task<IEnumerable<dynamic>> QueryEntities(string entityName, string condition)
        {
            var entities = await GetSerializedEntities(entityName, condition);
            return entities.Select(JsonConvert.DeserializeObject<dynamic>).ToList();
        }

        public async Task<IEnumerable<string>> GetEntityKeys(string entityName, string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return await GetAllEntitykeys(entityName);
            }

            var tokens = _conditionalTokenizer.Tokenize(condition);
            var parseTree = _whereParser.ParseCondition(tokens);
            return await ExecuteTree(entityName, parseTree);
        }

        private async Task<IEnumerable<string>> GetSerializedEntities(string entityName, string condition)
        {
            var keys = await GetEntityKeys(entityName, condition);

            ICollection<string> result = new List<string>();

            foreach (var key in keys)
            {
                result.Add(await RetrieveEntityJsonByKey(entityName, key));
            }

            return result;
        }

        public async Task<string> ConstructWhereStatementFromIdentifiers<TEntity>(TEntity entity)
        {
            bool KeyDefined(MemberInfo info) => info.IsDefined(Constants.KeyAttributeType);

            var entityType = typeof(TEntity);

            var entityName = entityType.Name.ToLower();

            var identifiers = new Dictionary<string, string>();

            entityType.GetFields().Where(KeyDefined).ToList().ForEach(x => identifiers.Add(x.Name, x.GetValue(entity).ToString()));

            entityType.GetProperties().Where(KeyDefined).ToList().ForEach(x => identifiers.Add(x.Name, x.GetValue(entity).ToString()));

            if (!identifiers.Any())
            {
                throw new KeyAttributeMissingException();
            }

            var whereStatement = string.Empty;

            var andKeyword = Keywords.And.ToString();

            foreach (var identifier in identifiers)
            {
                var value = identifier.Value;

                var propertyType = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), identifier.Key.ToLower());

                if (string.IsNullOrEmpty(propertyType))
                {
                    throw new NonExistentPropertyException(identifier.Key);
                }

                Enum.TryParse(propertyType, true, out TypeNames type);

                if (Helpers.QuotedValue(type))
                {
                    value = $"'{value}'";
                }

                whereStatement += $"{identifier.Key} = {value} {andKeyword} ";
            }

            return whereStatement.Substring(0, whereStatement.Length - 1 - (andKeyword.Length + 1));
        }

        private async Task<string> RetrieveEntityJsonByKey(string entityName, string key)
        {
            return await _stringClient.GetValue(Helpers.GetEntityStoreKey(entityName, key));
        }

        private async Task<IEnumerable<string>> GetAllEntitykeys(string entityName)
        {
            return await _setClient.GetSetMembers(Helpers.GetEntityIdentifierCollectionKey(entityName));
        }

        private async Task<string> ExecuteCondition(string entityName, string property, Operator op, string value)
        {
            property = property.Split('.').Last();
            var propertyTypeName = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), property);
            value = Helpers.EncodePropertyValue(propertyTypeName, value);
            var score = Helpers.GetPropertyScore(propertyTypeName, value);

            if (op == Operator.Equals)
            {
                return await GetKeysMatchingPropertyValue(entityName, property, value);
            }

            IEnumerable<string> range = new List<string>();

            if (op == Operator.GreaterThanOrEqualTo || op == Operator.GreaterThan)
            {
                range = await (score.HasValue? GetRangeByScore(entityName, property, score.Value, double.PositiveInfinity) 
                    : GetRangeByValue(entityName, property, value, string.Empty));

                range = op == Operator.GreaterThan ? range.Where(x => x != value) : range; //Filter equal values
            }

            if (op == Operator.LessThanOrEqualTo || op == Operator.LessThan)
            {
                range = await (score.HasValue? GetRangeByScore(entityName, property, double.NegativeInfinity, score.Value) 
                    :  GetRangeByValue(entityName, property, string.Empty, value));

                range = op == Operator.LessThan ? range.Where(x => x != value) : range; //Filter equal values
            }

            if (op == Operator.NotEqual)
            {
                range = (await GetRangeByValue(entityName, property, string.Empty, string.Empty)).Where(x => x != value);
            }

            return await MapRangeToKeys(range, entityName, property);
        }

        private async Task<IEnumerable<string>> GetRangeByValue(string entityName, string property, string minValue, string maxValue)
        {
            return (await _zSetClient.GetSortedSetElementsByValue(Helpers.GetPropertyCollectionKey(entityName, property), minValue, maxValue)).ToList();
        }

        private async Task<IEnumerable<string>> GetRangeByScore(string entityName, string property, double minScore, double maxScore)
        {
            return (await _zSetClient.GetSortedSetElementsByScore(Helpers.GetPropertyCollectionKey(entityName, property), minScore, maxScore)).ToList();
        }

        private async Task<string> MapRangeToKeys(IEnumerable<string> range, string entityName, string property)
        {
            var result = new List<string>();
            foreach (var item in range)
            {
                result.Add(await GetKeysMatchingPropertyValue(entityName, property, item));
            }
            return string.Join(",", result);
        }

        private async Task<string> GetKeysMatchingPropertyValue(string entityName, string property, string value)
        {
            return await _hashClient.GetHashField(Helpers.GetEntityIndexKey(entityName, property), value);
        }
    }
}
