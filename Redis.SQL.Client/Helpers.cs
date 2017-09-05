﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        
        internal static string GetEntityStoreKey(string entityName, string identifier) => entityName.ToLower() + "_" + identifier.ToLower();

        internal static string GetEntityIndexKey(string entityName, string field) => entityName.ToLower() + "_" + field.ToLower();

        internal static string GetEntityCountKey(string entityName) => entityName.ToLower() + "_" + Constants.EntityCountSuffix;
    }
}
