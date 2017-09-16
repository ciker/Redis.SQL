using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class CreationalLexicalTokenizer : ILexer
    {
        public IEnumerable<string> Tokenize(string statement)
        {
            var tokens = new List<string>();


            return tokens;
        }
    }
}
