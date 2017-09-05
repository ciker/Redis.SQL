using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlQueryEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly IRedisZSetStorageClient _zSetClient;

        internal RedisSqlQueryEngine()
        {
            _hashClient = new RedisHashStorageClient();
            _stringClient = new RedisStringStorageClient();
            _zSetClient = new RedisZSetStorageClient();
        }

        internal async Task<IEnumerable<string>> ExecuteCondition(string entityName, string property, string op, string value)
        {
            if (op == "=")
            {
                return await GetKeysMatchingPropertyValue(entityName, property, value);
            }

            if (op == ">=")
            {
                var range = await GetRange(entityName, property, value, string.Empty);
                return await MapRangeToKeys(range, entityName, property);
            }

            if (op == "<=")
            {
                var range = await GetRange(entityName, property, string.Empty, value);
                return await MapRangeToKeys(range, entityName, property);
            }

            if (op == ">")
            {
                var range = (await GetRange(entityName, property, value, string.Empty)).Where(x => x != value);
                return await MapRangeToKeys(range, entityName, property);
            }

            if (op == "<")
            {
                var range = (await GetRange(entityName, property, string.Empty, value)).Where(x => x != value);
                return await MapRangeToKeys(range, entityName, property);
            }

            return null;
        }

        private async Task<IEnumerable<string>> GetRange(string entityName, string property, string minValue, string maxValue)
        {
            return (await _zSetClient.GetSortedSetElementsByValue(Helpers.GetPropertyCollectionKey(entityName, property), minValue, maxValue)).ToList();
        }

        private async Task<IEnumerable<string>> MapRangeToKeys(IEnumerable<string> range, string entityName, string property)
        {
            var result = new List<string>();
            foreach (var item in range)
            {
                result.AddRange(await GetKeysMatchingPropertyValue(entityName, property, item));
            }
            return result;
        }

        private async Task<IEnumerable<string>> GetKeysMatchingPropertyValue(string entityName, string property, string value)
        {
            var keys = await _hashClient.GetHashField(Helpers.GetEntityIndexKey(entityName, property), value);
            return keys == null ? new string[] { } : keys.Split(',');
        }
    }
}
