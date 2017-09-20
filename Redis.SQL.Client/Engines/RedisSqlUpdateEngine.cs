using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlUpdateEngine : IUpdateEngine
    {
        private IQueryEngine _queryEngine;
        private ILexer _updateLexicalTokenizer;
        private ICustomizedParser _updateParser;

        internal RedisSqlUpdateEngine()
        {
            _updateLexicalTokenizer = new UpdateLexicalTokenizer();
            _updateParser = new UpdateParser();
            _queryEngine = new RedisSqlQueryEngine();
        }
    }
}
