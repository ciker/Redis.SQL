using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal partial class RedisSqlQueryEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisZSetStorageClient _zSetClient;
        private static readonly string[] Operators = {"<=", ">=", "<", ">", "!=", "="};

        internal RedisSqlQueryEngine()
        {
            _hashClient = new RedisHashStorageClient();
            _stringClient = new RedisStringStorageClient();
            _zSetClient = new RedisZSetStorageClient();
        }

        internal async Task<string> RetrieveEntityJsonByKey(string entityName, string key)
        {
            return await _stringClient.GetValue(Helpers.GetEntityStoreKey(entityName, key));
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
