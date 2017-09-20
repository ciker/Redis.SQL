using System.Collections.Generic;

namespace Redis.SQL.Client.Analyzer.Interfaces
{
    internal interface ILexer
    {
        IList<string> Tokenize(string statement);
    }
}
