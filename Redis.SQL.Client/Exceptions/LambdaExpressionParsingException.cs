using System;

namespace Redis.SQL.Client.Exceptions
{
    public class LambdaExpressionParsingException : Exception
    {
        public LambdaExpressionParsingException() : base("Error Parsing the Provided Lambda Expression")
        {

        }

        public LambdaExpressionParsingException(string text) : base($"Error Parsing the Provided Lambda Expression: {text}")
        {

        }
    }
}
