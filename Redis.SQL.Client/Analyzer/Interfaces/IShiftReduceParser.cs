using System.Collections.Generic;

namespace Redis.SQL.Client.Analyzer.Interfaces
{
    internal interface IShiftReduceParser
    {
        BinaryTree<string> ParseCondition(IEnumerable<string> tokens);
    }
}
