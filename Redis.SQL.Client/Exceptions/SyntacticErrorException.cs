using System;

namespace Redis.SQL.Client.Exceptions
{
    public class SyntacticErrorException : Exception
    {
        public SyntacticErrorException(string token) : base("Error parsing: " + token)
        {

        }
    }
}
