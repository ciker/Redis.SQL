using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Redis.SQL.Client
{
    internal class Helpers
    {
        internal static string GetTypeName<T>() => typeof(T).Name.ToLower();

        internal static string GenerateRandomString() => Guid.NewGuid().ToString().Replace("-", string.Empty);

        internal static IEnumerable<PropertyInfo> GetTypeProperties<T>() => typeof(T).GetProperties().ToList();

        internal static string GetDateTimeRedisValue(DateTime date)
        {
            var unixEpoch = new DateTime(1970, 1, 1);
            return date.Subtract(unixEpoch).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        internal static string GetTimeSpanRedisValue(TimeSpan time) => time.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

        internal static string GetBooleanRedisValue(bool value) => (value ? 0 : 1).ToString();

        internal static string GetEntityStoreKey(string entityName, string identifier) => entityName.ToLower() + Constants.EntityDataDirectoryName + identifier.ToLower();

        internal static string GetEntityIndexKey(string entityName, string property) => entityName.ToLower() + Constants.EntityIndexesDirectoryName + property.ToLower();

        internal static string GetPropertyCollectionKey(string entityName, string property) => entityName.ToLower() + Constants.EntityPropertyCollectionDirectoryName + property.ToLower();

        internal static string GetEntityCountKey(string entityName) => entityName.ToLower() + Constants.EntityCountSuffix;
    }
}