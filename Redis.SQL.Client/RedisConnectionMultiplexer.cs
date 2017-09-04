using StackExchange.Redis;

namespace Redis.SQL.Client
{
    internal sealed class RedisConnectionMultiplexer
    {
        private const string RedisConnectionKey = "RedisConnectionKey";

        private static RedisConnectionMultiplexer _multiplexer;

        internal readonly ConnectionMultiplexer Connection;

        private static readonly object Locker = new object();

        private RedisConnectionMultiplexer()
        {
            var configurationManager = new ConfigurationManager();
            Connection = ConnectionMultiplexer.Connect(configurationManager.GetConfigKey(RedisConnectionKey));
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