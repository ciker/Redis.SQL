using System;

namespace Redis.SQL.Client.Parsers
{
    public class SyntacticErrorException : Exception
    {
        public SyntacticErrorException(string message) : base(message)
        {
            
        }
    }
}
