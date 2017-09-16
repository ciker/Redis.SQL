using Redis.SQL.Client.Analyzer;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client
{
    internal class Constants
    {
        internal const string RedisConnectionKey = "RedisConnectionKey";

        internal const string RedisDatabaseIndex = "RedisDatabaseIndex";

        internal const string EntityCountKey = ":meta:entity_count";

        internal const string EntityIdentifierCollectionKey = ":meta:identifiers";

        internal const string EntityPropertyTypesKey = ":meta:property_types";

        internal const string EntityKeyFieldName = "entitykey";

        internal const string AllEntityNamesSetKeyName = "entities";

        internal const string EntityIndexesDirectoryName = ":indexes:";

        internal const string EntityPropertyCollectionDirectoryName = ":properties:";

        internal const string EntityDataDirectoryName = ":data:";

        internal const int DefaultDatabaseIndex = 0;

        internal const string WhereClauseTokenPattern = @"^(([\w-]+|[\w-]+.[\w-]+)(!=|=|>=|>|<|<=)+([\d.-]+|'.*'|[tT]rue|[fF]alse)+|[aA][nN][dD]|[oO][rR]|\(|\))$";

        internal const string ProjectionTokenPattern = @"^(([\w-]+|[\w-]+.[\w-]+)|\*)$";

        internal const string EntityNamePattern = @"^([\w-]+)$";

        internal const string EntityDesignPattern = @"^([\w-]+):([\w-]+)$";

        internal const string PropertyValuePattern = @"^([\d.-]+|'.*'|[tT][rR][uU][eE]|[fF][aA][lL][sS][eE])$";

        internal static readonly Grammar WhereGrammar = new Grammar(GrammarType.Where);
    }
}