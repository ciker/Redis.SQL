using System;
using Microsoft.Extensions.Configuration;

namespace Redis.SQL.Client
{
    internal class ConfigurationManager
    {
        private static IConfiguration _configuration;

        private static readonly object Locker = new object();

        internal ConfigurationManager()
        {
            if (_configuration != null) return;

            lock (Locker)
            {
                _configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", true, true).Build();
            }
        }

        internal string GetConfigKey(string key)
        {
            return _configuration[key];
        }
    }
}
