using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Redis.SQL.Client
{
    internal class Helpers
    {
        internal static string GetTypeName<T>()
        {
            return typeof(T).Name.ToLower();
        }

        internal static string GenerateRandomString()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        internal static IEnumerable<PropertyInfo> GetTypeProperties<T>() => typeof(T).GetProperties().ToList();

        internal static string GetDateTimeSortedSetValue(DateTime date)
        {
            var unixEpoch = new DateTime(1970, 1, 1);
            return date.Subtract(unixEpoch).TotalMilliseconds.ToString();
        }

        internal static string GetTimeSpanSortedSetValue(TimeSpan time) => time.TotalMilliseconds.ToString();

        internal static string GetBooleanSortedSetValue(bool value) => (value ? 0 : 1).ToString();

        internal static string GetEntityStoreKey(string entityName, string identifier) => entityName.ToLower() + "_" + identifier.ToLower();

        internal static string GetEntityIndexKey(string entityName, string field) => entityName.ToLower() + "_" + field.ToLower();

        internal static string GetEntityCountKey(string entityName) => entityName.ToLower() + "_" + Constants.EntityCountSuffix;
    }
}