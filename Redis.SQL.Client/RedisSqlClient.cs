using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer;
using Redis.SQL.Client.Engines;

namespace Redis.SQL.Client
{
    public class RedisSqlClient
    {
        private readonly LexicalTokenizer _lexicalTokenizer;

        private readonly ShiftReduceParser _shiftReduceParser;

        private readonly RedisSqlQueryEngine _queryEngine;

        public RedisSqlClient()
        {
            _lexicalTokenizer = new LexicalTokenizer();
            _shiftReduceParser = new ShiftReduceParser(Constants.WhereGrammar);
            _queryEngine = new RedisSqlQueryEngine();
        }

        public async Task ExecuteWhere(string condition)
        {
            var tokens = _lexicalTokenizer.Tokenize(condition);
            var parseTree = _shiftReduceParser.ParseCondition(tokens);
            await _queryEngine.ExecuteTree(parseTree);
        }

    }
}
