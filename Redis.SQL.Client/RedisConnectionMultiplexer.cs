using StackExchange.Redis;

namespace Redis.SQL.Client
{
    internal sealed class RedisConnectionMultiplexer
    {
        public static readonly string RedisConnectionKey = "RedisConnectionKey";

        private static RedisConnectionMultiplexer _multiplexer;

        internal readonly ConnectionMultiplexer Connection;

        private RedisConnectionMultiplexer()
        {
            Connection = ConnectionMultiplexer.Connect(RedisConnectionKey);
        }

        internal RedisConnectionMultiplexer GetMultiplexer()
        {
            return _multiplexer ?? (_multiplexer = new RedisConnectionMultiplexer());
        }
    }
}
