using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlUpdateEngine : IUpdateEngine
    {
        private readonly IQueryEngine _queryEngine;
        private readonly ILexer _updateLexicalTokenizer;
        private readonly ICustomizedParser _updateParser;

        internal RedisSqlUpdateEngine()
        {
            _updateLexicalTokenizer = new UpdateLexicalTokenizer();
            _updateParser = new UpdateParser();
            _queryEngine = new RedisSqlQueryEngine();
        }

        public Task ExecuteUpdateStatement(string sql)
        {
            var tokens = _updateLexicalTokenizer.Tokenize(sql);

            return null;
        }
    }
}
