using System;
using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Models;

namespace Redis.SQL.Client.Analyzer.Parsers
{
    internal class UpdateParser : ICustomizedParser
    {
        public BaseModel ParseTokens(IList<string> tokens)
        {
            throw new NotImplementedException();
        }
    }
}
