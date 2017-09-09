using Redis.SQL.Client.Analyzer;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client
{
    internal class Constants
    {
        internal const string RedisConnectionKey = "RedisConnectionKey";

        internal const string RedisDatabaseIndex = "RedisDatabaseIndex";

        internal const string EntityCountKey = ":meta:entity_count";

        internal const string EntityPropertyTypesKey = ":meta:property_types";

        internal const string EntityKeyFieldName = "entitykey";

        internal const string AllEntityNamesSetKeyName = "entities";

        internal const string EntityIndexesDirectoryName = ":indexes:";

        internal const string EntityPropertyCollectionDirectoryName = ":properties:";

        internal const string EntityDataDirectoryName = ":data:";

        internal const int DefaultDatabaseIndex = 0;

        internal const string WhereExpressionPattern = @"^(([\w-]+|[\w-]+.[\w-]+)(!=|=|>=|>|<|<=)+([\d.-]+|'[a-zA-Z0-9\s]+'|[tT][rR][uU][eE]|[fF][aA][lL][sS][eE])+)$";

        internal static readonly Grammar WhereGrammar = new Grammar(GrammarType.Where);
    }
}