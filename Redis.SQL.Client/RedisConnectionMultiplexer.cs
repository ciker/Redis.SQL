using System;
using StackExchange.Redis;

namespace Redis.SQL.Client
{
    internal sealed class RedisConnectionMultiplexer
    {
        private static RedisConnectionMultiplexer _multiplexer;

        internal readonly ConnectionMultiplexer Connection;

        private static readonly object Locker = new object();

        private RedisConnectionMultiplexer()
        {
            var redisConnectionKey = new ConfigurationManager().GetConfigKey(Constants.RedisConnectionKey);
            if (string.IsNullOrEmpty(redisConnectionKey))
            {
                throw new Exception("RedisConnectionKey should be defined in the application settings JSON file");
            }
            Connection = ConnectionMultiplexer.Connect(redisConnectionKey);
        }

        internal static RedisConnectionMultiplexer GetMultiplexer()
        {
            lock (Locker)
            {
                return _multiplexer ?? (_multiplexer = new RedisConnectionMultiplexer());
            }
        }
    }
}