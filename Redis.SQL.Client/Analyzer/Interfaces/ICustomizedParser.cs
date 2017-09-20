using System.Collections.Generic;
using Redis.SQL.Client.Models;

namespace Redis.SQL.Client.Analyzer.Interfaces
{
    internal interface ICustomizedParser
    {
        BaseModel ParseTokens(IList<string> tokens);
    }
}