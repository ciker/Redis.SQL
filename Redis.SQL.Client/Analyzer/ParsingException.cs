using System;

namespace Redis.SQL.Client.Analyzer
{
    public class ParsingException : Exception
    {
        public ParsingException() : base("Error Parsing the Provided Expression")
        {
            
        }
    }
}
