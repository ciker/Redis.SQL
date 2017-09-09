using Redis.SQL.Client.Analyzer;

namespace Redis.SQL.Client
{
    public class RedisSqlClient
    {
        private readonly LexicalTokenizer _lexicalTokenizer;

        private readonly ShiftReduceParser _shiftReduceParser;

        public RedisSqlClient()
        {
            _lexicalTokenizer = new LexicalTokenizer(Constants.WhereClauseTokenPattern);
            _shiftReduceParser = new ShiftReduceParser(Constants.WhereGrammar);
        }

        public void ExecuteWhere(string condition)
        {
            var tokens = _lexicalTokenizer.Tokenize(condition);
            var parseTree = _shiftReduceParser.ParseCondition(tokens);
        }

    }
}
