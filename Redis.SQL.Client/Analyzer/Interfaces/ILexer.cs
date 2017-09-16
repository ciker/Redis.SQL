using System.Collections.Generic;

namespace Redis.SQL.Client.Analyzer.Interfaces
{
    internal interface ILexer
    {
        IEnumerable<string> Tokenize(string statement);
    }
}
