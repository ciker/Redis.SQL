
namespace Redis.SQL.Client
{
    internal class Constants
    {
        internal const string RedisConnectionKey = "RedisConnectionKey";

        internal const string RedisDatabaseIndex = "RedisDatabaseIndex";

        internal const string EntityCountSuffix = ":meta:entity_count";

        internal const string EntityKeyFieldName = "entitykey";

        internal const string AllEntityNamesSetKeyName = "entities";

        internal const string EntityIndexesDirectoryName = ":indexes:";

        internal const string EntityPropertyCollectionDirectoryName = ":properties:";

        internal const string EntityDataDirectoryName = ":data:";

        internal const int DefaultDatabaseIndex = 0;
    }
}
