﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Enums;
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

        internal async Task<IEnumerable<string>> ExecuteCondition(string entityName, string property, Operator op, string value)
        {
            value = await ResolvePropertyValue(entityName, property, value.ToLower());

            if (op == Operator.Equals)
            {
                return await GetKeysMatchingPropertyValue(entityName, property, value);
            }

            if (op == Operator.GreaterThanOrEqualTo)
            {
                var range = await GetRange(entityName, property, value, string.Empty);
                return await MapRangeToKeys(range, entityName, property);
            }

            if (op == Operator.LessThanOrEqualTo)
            {
                var range = await GetRange(entityName, property, string.Empty, value);
                return await MapRangeToKeys(range, entityName, property);
            }

            if (op == Operator.GreaterThan)
            {
                var range = (await GetRange(entityName, property, value, string.Empty)).Where(x => x != value);
                return await MapRangeToKeys(range, entityName, property);
            }

            if (op == Operator.LessThan)
            {
                var range = (await GetRange(entityName, property, string.Empty, value)).Where(x => x != value);
                return await MapRangeToKeys(range, entityName, property);
            }

            if (op == Operator.NotEqual)
            {
                var range = (await GetRange(entityName, property, string.Empty, string.Empty)).Where(x => x != value);
                return await MapRangeToKeys(range, entityName, property);
            }

            return null;
        }

        private async Task<string> ResolvePropertyValue(string entityName, string property, string value)
        {
            var propertyType = await _hashClient.GetHashField(Helpers.GetEntityPropertyTypesKey(entityName), property);
            if (propertyType == typeof(DateTime).Name)
            {
                return Helpers.GetDateTimeRedisValue(DateTime.Parse(value));
            }

            if (propertyType == typeof(TimeSpan).Name)
            {
                return Helpers.GetTimeSpanRedisValue(TimeSpan.Parse(value));
            }

            if (propertyType == typeof(bool).Name)
            {
                if (string.Equals(value, RedisBoolean.True.ToString(), StringComparison.OrdinalIgnoreCase)) return ((long)RedisBoolean.True).ToString();
                if (string.Equals(value, RedisBoolean.False.ToString(), StringComparison.OrdinalIgnoreCase)) return ((long)RedisBoolean.False).ToString();

                return Helpers.GetBooleanRedisValue(bool.Parse(value));
            }

            return value;
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