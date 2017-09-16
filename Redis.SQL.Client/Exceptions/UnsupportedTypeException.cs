using System;

namespace Redis.SQL.Client.Exceptions
{
    public class UnsupportedTypeException : Exception
    {
        public UnsupportedTypeException(string type) : base($"Error, the type: {type} is unsupported by the engine.")
        {

        }
    }
}