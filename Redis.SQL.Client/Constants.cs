﻿
namespace Redis.SQL.Client
{
    internal class Constants
    {
        internal const string RedisConnectionKey = "RedisConnectionKey";

        internal const string RedisDatabaseIndex = "RedisDatabaseIndex";

        internal const string EntityCountSuffix = ":entity_count";

        internal const string EntityKeyFieldName = "entitykey";

        internal const string EntityIndexesDirectoryName = ":indexes:";

        internal const string EntityDataDirectoryName = ":data:";

        internal const int DefaultDatabaseIndex = 0;
    }
}
