using System;

namespace Redis.SQL.Client.Exceptions
{
    public class IncompatibleTypesException : Exception
    {
        internal IncompatibleTypesException(string type, string value) : base($"The value {value} is incompatible with the type {type}")
        {
            
        }
    }
}