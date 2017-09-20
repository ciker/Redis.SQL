using System;

namespace Redis.SQL.Client.Exceptions
{
    public class NonExistentPropertyException : Exception
    {
        public NonExistentPropertyException(string property) : base($"Non-Existent Property: {property}")
        {
            
        }
    }
}
