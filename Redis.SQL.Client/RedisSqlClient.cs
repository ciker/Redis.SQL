using System.Diagnostics;
using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer;
using Redis.SQL.Client.Engines;

namespace Redis.SQL.Client
{
    public class RedisSqlClient
    {
        private readonly LexicalTokenizer _whereTokenizer;

        private readonly ShiftReduceParser _whereParser;

        private readonly RedisSqlQueryEngine _queryEngine;

        public RedisSqlClient()
        {
            _whereTokenizer = new LexicalTokenizer(Constants.WhereClauseTokenPattern);
            _whereParser = new ShiftReduceParser(Constants.WhereGrammar);
            _queryEngine = new RedisSqlQueryEngine();
        }


        public async Task ExecuteWhere(string entityName, string condition)
        {
            var tokens = _whereTokenizer.Tokenize(condition);
            var parseTree = _whereParser.ParseCondition(tokens);
            await _queryEngine.ExecuteTree(entityName, parseTree);
        }
    }
}
