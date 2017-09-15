using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client
{
    internal class Helpers
    {
        internal static string GetTypeName<T>() => typeof(T).Name.ToLower();

        internal static string GenerateRandomString() => Guid.NewGuid().ToString().Replace("-", string.Empty);

        internal static IEnumerable<PropertyInfo> GetTypeProperties<T>() => typeof(T).GetProperties().ToList();
        
        internal static string GetEntityStoreKey(string entityName, string identifier) => entityName.ToLower() + Constants.EntityDataDirectoryName + identifier.ToLower();

        internal static string GetEntityIndexKey(string entityName, string property) => entityName.ToLower() + Constants.EntityIndexesDirectoryName + property.ToLower();

        internal static string GetPropertyCollectionKey(string entityName, string property) => entityName.ToLower() + Constants.EntityPropertyCollectionDirectoryName + property.ToLower();

        internal static string GetEntityCountKey(string entityName) => entityName.ToLower() + Constants.EntityCountKey;

        internal static string GetEntityIdentifierCollectionKey(string entityName) => entityName.ToLower() + Constants.EntityIdentifierCollectionKey;

        internal static string GetEntityPropertyTypesKey(string entityName) => entityName.ToLower() + Constants.EntityPropertyTypesKey;

        internal static string SerializeRedisValue<T>(T value) => typeof(T) == typeof(string) ? value.ToString() : JsonConvert.SerializeObject(value);

        internal static string EncodePropertyValue(string propertyTypeName, string value)
        {
            if (propertyTypeName == TypeNames.DateTime.ToString())
            {
                if (DateTime.TryParse(value, new CultureInfo("en-US"), DateTimeStyles.None, out var parsed))
                {
                    return GetDateTimeRedisValue(parsed);
                }

                goto Error;
            }

            if (propertyTypeName == TypeNames.TimeSpan.ToString())
            {
                if (TimeSpan.TryParse(value, out var parsed))
                {
                    return GetTimeSpanRedisValue(parsed);
                }

                goto Error;
            }

            if (propertyTypeName == TypeNames.Boolean.ToString())
            {
                switch (value.Trim())
                {
                    case "0":
                    case "1": return value; 
                }

                if (bool.TryParse(value, out var parsed))
                {
                    return GetBooleanRedisValue(parsed);
                }
            }

            return value;

        Error:
            throw new SyntacticErrorException($"{propertyTypeName} = {value}");
        }

        internal static double? GetPropertyScore(string propertyTypeName, string encodedPropertyValue)
        {
            if (propertyTypeName == TypeNames.String.ToString() || propertyTypeName == TypeNames.Char.ToString())
            {
                return null;
            }
            return double.Parse(encodedPropertyValue);
        }

        private static string GetDateTimeRedisValue(DateTime date)
        {
            var unixEpoch = new DateTime(1970, 1, 1);
            return date.Subtract(unixEpoch).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetTimeSpanRedisValue(TimeSpan time) => time.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

        private static string GetBooleanRedisValue(bool value) => (value ? (int)RedisBoolean.True : (int)RedisBoolean.False).ToString();
    }
}