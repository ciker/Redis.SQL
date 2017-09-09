using System;

namespace Redis.SQL.Client.Analyzer
{
    public class SyntacticErrorException : Exception
    {
        public SyntacticErrorException(string token) : base("Error parsing: " + token)
        {

        }
    }
}
