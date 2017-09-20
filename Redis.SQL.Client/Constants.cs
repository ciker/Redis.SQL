using System;
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

        internal const string ProjectionTokenPattern = @"^(([\w-]+|[\w-]+.[\w-]+)|\*)$";

        internal const string EntityNamePattern = @"^([\w-]+)$";

        internal const string EntityDesignPattern = @"^([\w-]+):([\w-]+)$";

        internal const string PropertyValuePattern = @"^([\d.-]+|'.*'|[tT][rR][uU][eE]|[fF][aA][lL][sS][eE])$";


        internal const string WhereClauseTokenPattern = @"^(([\w-]+|[\w-]+.[\w-]+)(!=|=|>=|>|<|<=)+([\d.-]+|'.*'|[tT]rue|[fF]alse)+|[aA][nN][dD]|[oO][rR]|\(|\))$";

        internal const string InsertStatementPattern = @"^[iI][nN][sS][eE][rR][tT][\s]+([\w-]+)[\s]*[\(](([\w-]+)|([\w-]+)[\s]*[\,][\s]*)+[\)][\s]*[vV][aA][lL][uU][eE][sS][\s]*[\(]([\s]*|[\,]|[\d.-]+|'.*'|[tT][rR][uU][eE]|[fF][aA][lL][sS][eE])+[\)]$";

        internal const string SelectStatementPattern = @"^[sS][eE][lL][eE][cC][tT](([\s]*[\*])|((([\s]+[\w-]+|[\s]+[\w-]+.[\w-]+)|([\s]+[\w-]+[\,]+|[\s]+[\w-]+.[\w-]+[\,]+))*))[\s]+[fF][rR][oO][mM][\s]+([\w-]+)([\s]+[wW][hH][eE][rR][eE]([\s]|\()(.+))*$";

        internal const string DeleteStatementPattern = @"^[dD][eE][lL][eE][tT][eE][\s]+([\w-]+)([\s]+[wW][hH][eE][rR][eE]([\s]|\()(.+))*$";

        internal const string CreateStatementPattern = @"^[cC][rR][eE][aA][tT][eE][\s]+([\w-]+)[\s]*([\(](([\s]*([\w-]+)[\s]*[\:][\s]*([\w-]+)[\s]*)|([\s]*([\w-]+)[\s]*[\:][\s]*([\w-]+)[\s]*[,]))+[\)])+$";

        internal static readonly Grammar WhereGrammar = new Grammar(GrammarType.Where);

        internal static Type UniqueIdentifierType = typeof(UniqueIdentifier);
    }
}