using System;

namespace Redis.SQL.Client.Analyzer
{
    public class SyntacticErrorException : Exception
    {
        public SyntacticErrorException(string message) : base(message)
        {
            
        }
    }
}
